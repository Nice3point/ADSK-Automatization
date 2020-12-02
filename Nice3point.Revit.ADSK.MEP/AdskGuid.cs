using System;

namespace Nice3point.Revit.ADSK.MEP
{
    public static class AdskGuid
    {
        public static readonly Guid AdskPosition = new Guid("ae8ff999-1f22-4ed7-ad33-61503d85f0f4"); //Позиция
        public static readonly Guid AdskName = new Guid("e6e0f5cd-3e26-485b-9342-23882b20eb43"); //Наименование
        public static readonly Guid AdskType = new Guid("2204049c-d557-4dfc-8d70-13f19715e46d"); //Тип,Марка
        public static readonly Guid AdskCode = new Guid("2fd9e8cb-84f3-4297-b8b8-75f444e124ed"); //Код оборудования
        public static readonly Guid AdskManufacturer = new Guid("a8cdbf7b-d60a-485e-a520-447d2055f351"); //Завод изготовитель
        public static readonly Guid AdskUnit = new Guid("4289cb19-9517-45de-9c02-5a74ebf5c86d"); //Единица измерения
        public static readonly Guid AdskQuantity = new Guid("8d057bb3-6ccd-4655-9165-55526691fe3a"); //Кол-во
        public static readonly Guid AdskMass = new Guid("32989501-0d17-4916-8777-da950841c6d7"); //Масса единицы
        public static readonly Guid AdskMassDimension = new Guid("5913a1f9-0b38-4364-96fe-a6f3cb7fcc68"); //Масса  с размерностью
        public static readonly Guid AdskNote = new Guid("a85b7661-26b0-412f-979c-66af80b4b2c3"); //Примечание
    }
}