using System;
using Autodesk.Revit.DB;

namespace AdskTemplateMepTools.RevitUtils
{
    public static class TransactionManager
    {
        private static readonly object SingleLocker = new();
        private static readonly object GroupLocker = new();

        public static void CreateTransaction(Document document, string transactionName, Action action)
        {
            lock (SingleLocker)
            {
                using var transaction = new Transaction(document);
                transaction.Start(transactionName);
                try
                {
                    action?.Invoke();
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.RollBack();
                }
            }
        }

        public static void CreateTransactionGroup(Document document, string transactionName, Action action)
        {
            lock (GroupLocker)
            {
                using var transaction = new TransactionGroup(document);
                transaction.Start(transactionName);
                try
                {
                    action?.Invoke();
                    transaction.Assimilate();
                }
                catch (Exception)
                {
                    transaction.RollBack();
                }
            }
        }
    }
}