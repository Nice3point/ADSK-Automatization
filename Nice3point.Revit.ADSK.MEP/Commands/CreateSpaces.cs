using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace Nice3point.Revit.ADSK.MEP.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class CreateSpaces : IExternalCommand
    {
        private static ICollection<ElementId> _warnElements = new List<ElementId>();
        private readonly Stopwatch _sTimer = new Stopwatch();
        private Document _doc;
        private List<LevelsData> _levelsDataCreation;
        private Document _linkedDoc;
        private List<RoomsData> _roomsDataList;
        private List<SpacesData> _spacesDataList;
        private Transform _tGlobal;
        private UIDocument _uiDoc;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            //Check if Document is opened in UI and Document is Project
            _sTimer.Reset();
            _uiDoc = commandData.Application.ActiveUIDocument;
            _doc = commandData.Application.ActiveUIDocument.Document;
            if (null == _uiDoc || _doc.IsFamilyDocument) return Result.Failed;
            //Start taskDialog for select link
            if (TdSelectArLink() == 0) return Result.Cancelled;

            var arLink = GetArLinkReference();
            if (null == arLink) return Result.Cancelled;
            //Get value for Room bounding in RevitLinkType
            if (_doc.GetElement(arLink.ElementId) is RevitLinkInstance selInstance)
            {
                var boundingWalls =
                    _doc.GetElement(selInstance.GetTypeId()) is RevitLinkType lnkType &&
                    Convert.ToBoolean(lnkType.get_Parameter(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING).AsInteger());
                //Get coordination transformation for link
                _tGlobal = selInstance.GetTotalTransform();
                //Get Document from RvtLinkInstance
                _linkedDoc = selInstance.GetLinkDocument();
                //Check for valid Document and Room bounding value checked
                if (null != _linkedDoc && !boundingWalls)
                {
                    TaskDialog.Show("Ошибка",
                        "Нет загруженной связи АР или в связанном файле не включен поиск границ помещения!\nДля размещения пространств необходимо включить этот параметр");
                    return Result.Succeeded;
                }
            }

            //Mainline code
            //Get placed Spaces and Levels information
            _spacesDataList = GetSpaces(_doc);
            _roomsDataList = GetRooms(_linkedDoc);
            //Check if Spaces placed
            if (_roomsDataList.Count > 0)
            {
                if (_spacesDataList.Count == 0)
                    switch (TdFirstTime())
                    {
                        case 0:
                            return Result.Succeeded;
                        case 1:
                            AnaliseAr();
                            break;
                        default:
                            return Result.Succeeded;
                    }
                else
                    AnaliseAr();
            }
            else
            {
                var tDialog = new TaskDialog("No Rooms in link")
                {
                    Title = "Нет помещений",
                    MainInstruction = "В выбранном экземпляре связи нет помещений.",
                    MainIcon = TaskDialogIcon.TaskDialogIconWarning,
                    TitleAutoPrefix = false,
                    CommonButtons = TaskDialogCommonButtons.Close
                };
                tDialog.Show();
            }

            return Result.Succeeded;
        }

        private void AnaliseAr()
        {
            _levelsDataCreation = MatchLevels(_roomsDataList.Select(r => r.RoomsLevel).ToList());
            if (_levelsDataCreation.Count > 0)
                switch (TdLevelsCreate())
                {
                    case 0:
                        return;
                    case 1:
                        _sTimer.Start();
                        CreateLevels(_levelsDataCreation);
                        _sTimer.Stop();
                        break;
                }

            switch (TdSpacesPlace())
            {
                case 0:
                    return;
                case 1:
                    _sTimer.Start();
                    CreateSpByRooms(true);
                    break;
                case 2:
                    _sTimer.Start();
                    CreateSpByRooms(false);
                    break;
            }
        }

        private void CreateSpByRooms(bool roomLimits)
        {
            var sCreated = 0;
            var sUpdated = 0;
            const double defLimitOffset = 3500d;
            using (var crTrans = new TransactionGroup(_doc, "Создание пространств"))
            {
                crTrans.Start();
                foreach (var roomsData in _roomsDataList)
                {
                    var lLevel = roomsData.RoomsLevel;
                    var localLevel = GetLevelByElevation(_doc, lLevel.Elevation);

                    if (null == localLevel) continue;
                    foreach (var lRoom in roomsData.RoomsList)
                    {
                        var roomName = lRoom.get_Parameter(BuiltInParameter.ROOM_NAME).AsString();
                        var roomNumber = lRoom.get_Parameter(BuiltInParameter.ROOM_NUMBER).AsString();
                        if (!lRoom.Level.Elevation.Equals(localLevel.Elevation)) continue;
                        using var tr = new Transaction(_doc, "Create space");
                        tr.Start();
                        var failOpt = tr.GetFailureHandlingOptions();
                        failOpt.SetFailuresPreprocessor(new SpaceExistWarner());
                        tr.SetFailureHandlingOptions(failOpt);
                        var lp = lRoom.Location as LocationPoint;
                        var rCoordinates = _tGlobal.OfPoint(lp?.Point);
                        var spLocPoint = new UV(rCoordinates.X, rCoordinates.Y);
                        var sp = _doc.Create.NewSpace(localLevel, spLocPoint);
                        var trStat = tr.Commit(failOpt);
                        //Space not exists, change name and number for new space
                        if (trStat == TransactionStatus.Committed)
                        {
                            tr.Start();
                            sp.get_Parameter(BuiltInParameter.ROOM_NAME).Set(roomName);
                            sp.get_Parameter(BuiltInParameter.ROOM_NUMBER).Set(roomNumber);
                            //TryToRenameSpace(tr,sp,RoomName,RoomNumber);
                            if (roomLimits)
                            {
                                var upperLevel = GetLevelByElevation(_doc, lRoom.UpperLimit.Elevation);
                                if (null == upperLevel)
                                {
                                    upperLevel = Level.Create(_doc, lRoom.UpperLimit.Elevation);
                                    upperLevel.Name = "АР_" + lRoom.UpperLimit.Name;
                                }
                            }
                            else
                            {
                                if (roomsData.RoomsLevel == roomsData.UpperRoomLevel)
                                {
                                    //sp.get_Parameter(BuiltInParameter.ROOM_UPPER_LEVEL).Set(localLevel.LevelId);
                                    sp.UpperLimit = GetLevelByElevation(_doc, localLevel.Elevation);
                                    sp.get_Parameter(BuiltInParameter.ROOM_UPPER_OFFSET)
                                        .Set(UnitUtils.ConvertToInternalUnits(defLimitOffset,
                                            DisplayUnitType.DUT_MILLIMETERS));
                                }
                                else
                                {
                                    var upperLevel = roomsData.UpperRoomLevel;
                                    sp.UpperLimit = GetLevelByElevation(_doc, upperLevel.Elevation);
                                    sp.get_Parameter(BuiltInParameter.ROOM_UPPER_OFFSET).Set(0);
                                }
                            }

                            tr.Commit(failOpt);
                            sCreated++;
                        }
                        //If Space placed in same area. Transaction creating space RolledBack
                        else
                        {
                            foreach (var eId in _warnElements)
                            {
                                if (null == _doc.GetElement(eId) || !(_doc.GetElement(eId) is Space)) continue;
                                var wSpace = _doc.GetElement(eId) as Space;
                                //bool updated = false;
                                var sLp = wSpace.Location as LocationPoint;

                                tr.Start();
                                sLp.Point = rCoordinates;
                                wSpace.get_Parameter(BuiltInParameter.ROOM_NAME).Set(roomName);
                                wSpace.get_Parameter(BuiltInParameter.ROOM_NUMBER).Set(roomNumber);
                                if (roomLimits)
                                {
                                    var upperLevel = GetLevelByElevation(_doc, lRoom.UpperLimit.Elevation);
                                    if (null == upperLevel)
                                    {
                                        upperLevel = Level.Create(_doc, lRoom.UpperLimit.Elevation);
                                        upperLevel.Name = "АР_" + lRoom.UpperLimit.Name;
                                    }

                                    wSpace.UpperLimit = upperLevel;
                                    wSpace.get_Parameter(BuiltInParameter.ROOM_UPPER_OFFSET).Set(lRoom.LimitOffset);
                                }
                                else
                                {
                                    if (roomsData.RoomsLevel == roomsData.UpperRoomLevel)
                                    {
                                        var upperLevel = localLevel;
                                        wSpace.UpperLimit = upperLevel;
                                        wSpace.get_Parameter(BuiltInParameter.ROOM_UPPER_OFFSET)
                                            .Set(UnitUtils.ConvertToInternalUnits(defLimitOffset,
                                                DisplayUnitType.DUT_MILLIMETERS));
                                    }
                                    else
                                    {
                                        var upperLevel = roomsData.UpperRoomLevel;
                                        wSpace.UpperLimit = GetLevelByElevation(_doc, upperLevel.Elevation);
                                        wSpace.get_Parameter(BuiltInParameter.ROOM_UPPER_OFFSET).Set(0);
                                    }
                                }

                                tr.Commit();
                                sUpdated++;
                            }
                        }
                    }
                }

                crTrans.Assimilate();
            }

            _sTimer.Stop();
            var resTimer = _sTimer.Elapsed;
            var tDialog = new TaskDialog("Result data")
            {
                Title = "Отчёт",
                MainInstruction = $"Обновлено {sUpdated}\nСоздано {sCreated}",
                MainIcon = TaskDialogIcon.TaskDialogIconShield,
                TitleAutoPrefix = false,
                FooterText = $"Общее время {resTimer.Minutes:D2}:{resTimer.Seconds:D2}:{resTimer.Milliseconds:D3}",
                CommonButtons = TaskDialogCommonButtons.Close
            };
            tDialog.Show();
        }

        private static Level GetLevelByElevation(Document doc, double elevation)
        {
            return new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .FirstOrDefault(l => l.Elevation.Equals(elevation));
        }

        private static int TdSpacesPlace()
        {
            var td = new TaskDialog("Spaces place Type")
            {
                Id = "ID_TaskDialog_Spaces_Type_Place",
                MainIcon = TaskDialogIcon.TaskDialogIconInformation,
                Title = "Создание/обновление пространств",
                TitleAutoPrefix = false,
                AllowCancellation = true,
                MainInstruction = "Настройка задания верхнего предела и смещения для размещения пространств",
                MainContent = "Выберите способ создания/обновления пространств",
                ExpandedContent =
                    "При выборе варианта по помещениям - пространства создаются с копированием всех настроект для привязки верхнего уровня и смещения из помещений. Так же будут дополнительно скопированы уровни для верхнего предела, если таковые отсутсвуют.\n" +
                    "При выборе варианта по уровням - верхним пределом для пространств будет следующий уровень с помещениями и значение смещения 0 мм. Если такового не существует (последний уровень с помещениями) используется смещение 3500 мм."
            };
            // Message related stuffs
            // Command link stuffs
            td.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "По помещениям");
            td.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "По уровням");
            // Dialog showUp stuffs
            var tdRes = td.Show();
            return tdRes switch
            {
                TaskDialogResult.Cancel => 0,
                TaskDialogResult.CommandLink1 => 1,
                TaskDialogResult.CommandLink2 => 2,
                _ => throw new Exception("Invalid value for TaskDialogResult")
            };
        }

        private static int TdLevelsCreate()
        {
            var td = new TaskDialog("Levels Create for Spaces")
            {
                Id = "ID_TaskDialog_Create_Levels",
                MainIcon = TaskDialogIcon.TaskDialogIconInformation,
                Title = "Создание недостающих уровней",
                TitleAutoPrefix = false,
                AllowCancellation = true,
                MainInstruction = "В текущем проекте не хватет уровней для создания инженерных пространств",
                MainContent = "Создать уровни в текущем проекте",
                ExpandedContent =
                    "В выбранном файле архитектуры имеются помещения, размещенные на уровнях, отсутсвующих в текущем проекте.\nНеобходимо создать уровни в текущем проекте для автоматического размещения инженерных пространств\n" +
                    "Будут созданы уровни с префиксом АР_имя уровня для всех помещений"
            };

            // Message related stuffs
            // Command link stuffs
            td.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Продолжить");
            // Dialog showUp stuffs
            var tdRes = td.Show();
            return tdRes switch
            {
                TaskDialogResult.Cancel => 0,
                TaskDialogResult.CommandLink1 => 1,
                _ => throw new Exception("Invalid value for TaskDialogResult")
            };
        }

        private static int TdFirstTime()
        {
            var td = new TaskDialog("First time Spaces placing")
            {
                Id = "ID_TaskDialog_Place_Spaces_Type",
                MainIcon = TaskDialogIcon.TaskDialogIconInformation,
                Title = "Размещение инженерных пространств",
                TitleAutoPrefix = false,
                AllowCancellation = true,
                MainInstruction = "В текущем проекте нет размещенных инженерных пространств",
                MainContent = "Проанализировать связанный файл на наличие помещений"
            };
            // Message related stuffs
            // Command link stuffs
            td.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Продолжить");
            // Dialog showUp stuffs
            var tdRes = td.Show();
            return tdRes switch
            {
                TaskDialogResult.Cancel => 0,
                TaskDialogResult.CommandLink1 => 1,
                _ => throw new Exception("Invalid value for TaskDialogResult")
            };
        }

        private static int TdSelectArLink()
        {
            var td = new TaskDialog("Select Link File")
            {
                Id = "ID_TaskDialog_Select_AR",
                MainIcon = TaskDialogIcon.TaskDialogIconInformation,
                Title = "Выбор экземпляра размещенной связи",
                TitleAutoPrefix = false,
                AllowCancellation = true,
                MainInstruction = "Выберите экземпляр размещенной связи для поиска помещений"
            };

            // Message related stuffs
            // Command link stuffs
            td.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Выбрать");
            // Dialog showUp stuffs
            var tdRes = td.Show();
            return tdRes switch
            {
                TaskDialogResult.Cancel => 0,
                TaskDialogResult.CommandLink1 => 1,
                _ => throw new Exception("Invalid value for TaskDialogResult")
            };
        }

        private Reference GetArLinkReference()
        {
            var arSelection = _uiDoc.Selection;
            try
            {
                return arSelection.PickObject(ObjectType.Element, new RvtLinkInstanceFilter(),
                    "Выберите экземпляр размещенной связи АР");
            }
            catch (Exception)
            {
                //user abort selection or other
                return null;
            }
        }

        private class SpaceExistWarner : IFailuresPreprocessor
        {
            public FailureProcessingResult PreprocessFailures(FailuresAccessor a)
            {
                var failures = a.GetFailureMessages();
                foreach (var f in failures)
                {
                    var id = f.GetFailureDefinitionId();
                    if (BuiltInFailures.GeneralFailures.DuplicateValue == id) a.DeleteWarning(f);

                    if (BuiltInFailures.RoomFailures.RoomsInSameRegionSpaces != id) continue;
                    _warnElements = f.GetFailingElementIds();
                    a.DeleteWarning(f);
                    return FailureProcessingResult.ProceedWithRollBack;
                }

                return FailureProcessingResult.Continue;
            }
        }

        private class RvtLinkInstanceFilter : ISelectionFilter
        {
            public bool AllowElement(Element element)
            {
                return element is RevitLinkInstance;
            }

            public bool AllowReference(Reference refer, XYZ point)
            {
                return false;
            }
        }

        #region Methods for Classes

        private static List<RoomsData> GetRooms(Document linkedDoc)
        {
            var arRooms = new List<RoomsData>();
            var arLevels = GetLevels(linkedDoc).OrderBy(l => l.Elevation).ToList();
            for (var i = 0; i < arLevels.Count; i++)
            {
                var roomsInLevel = GetRoomsByLevel(linkedDoc, arLevels[i]);
                if (roomsInLevel.Count <= 0) continue;
                var upperLevel = arLevels[i];
                var nextLevel = i + 1;
                while (nextLevel < arLevels.Count && GetRoomsByLevel(linkedDoc, arLevels[nextLevel]).Count == 0)
                    nextLevel++;

                if (nextLevel < arLevels.Count) upperLevel = arLevels[nextLevel];

                arRooms.Add(new RoomsData(arLevels[i], upperLevel, roomsInLevel));
            }

            return arRooms;
        }

        private static List<SpacesData> GetSpaces(Document currentDoc)
        {
            var curLevels = GetLevels(currentDoc);
            return curLevels.Select(curLevel => new {curLevel, spacesInLevel = GetSpacesByLevel(currentDoc, curLevel)})
                .Where(t => t.spacesInLevel.Count > 0)
                .Select(t => new SpacesData(t.curLevel, t.spacesInLevel))
                .ToList();
        }

        private static List<Room> GetRoomsByLevel(Document doc, Element level)
        {
            return new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.OST_Rooms)
                .Cast<Room>()
                .Where(e => e.Level.Id.IntegerValue.Equals(level.Id.IntegerValue) && e.Area > 0)
                .ToList();
        }

        private static List<Space> GetSpacesByLevel(Document doc, Element level)
        {
            return new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.OST_MEPSpaces)
                .Cast<Space>()
                .Where(e => e.Level.Id.IntegerValue.Equals(level.Id.IntegerValue) && e.Volume != 0)
                .ToList();
        }

        private static IEnumerable<Level> GetLevels(Document doc)
        {
            return new FilteredElementCollector(doc)
                .OfCategory(BuiltInCategory.OST_Levels)
                .WhereElementIsNotElementType()
                .Cast<Level>()
                .ToList();
        }

        private List<LevelsData> MatchLevels(IEnumerable<Level> linkedLevelList)
        {
            return linkedLevelList.Where(checkedLevel => GetRoomsByLevel(_linkedDoc, checkedLevel).Any())
                .Where(checkedLevel => null == GetLevelByElevation(_doc, checkedLevel.Elevation))
                .Select(checkedLevel => new LevelsData(checkedLevel.Name, checkedLevel.Elevation,
                    GetRoomsByLevel(_linkedDoc, checkedLevel).Count))
                .ToList();
        }

        private void CreateLevels(IEnumerable<LevelsData> elevList)
        {
            using var trLevels = new Transaction(_doc, "Создание уровней");
            trLevels.Start();
            foreach (var lData in elevList)
            {
                using var sLevel = new SubTransaction(_doc);
                sLevel.Start();
                var newLevel = Level.Create(_doc, lData.LevelElevation);
                newLevel.Name =
                    "АР_" + lData
                        .LevelName; //"АР_"+UnitUtils.ConvertFromInternalUnits(elevation, DisplayUnitType.DUT_MILLIMETERS);
                sLevel.Commit();
            }

            trLevels.Commit();
        }

        #endregion

        #region Classes

        private class RoomsData
        {
            public RoomsData(Level level, Level upperRoomLevel, List<Room> roomsList)
            {
                RoomsLevel = level;
                RoomsList = roomsList;
                UpperRoomLevel = upperRoomLevel;
            }

            public Level RoomsLevel { get; }
            public Level UpperRoomLevel { get; }
            public List<Room> RoomsList { get; }
        }

        private class SpacesData
        {
            private readonly Level _level;
            private readonly List<Space> _spacesList;

            public SpacesData(Level level, List<Space> spacesList)
            {
                _level = level;
                _spacesList = spacesList;
            }
        }

        private class LevelsData
        {
            public LevelsData(string levelName, double levelElevation, int elementCount)
            {
                LevelName = levelName;
                LevelElevation = levelElevation;
                ElementsCount = elementCount;
            }

            public string LevelName { get; }
            public double LevelElevation { get; }
            private int ElementsCount { get; }
        }

        #endregion
    }
}