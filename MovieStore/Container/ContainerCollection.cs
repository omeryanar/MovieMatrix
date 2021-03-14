using System;
using System.Collections.ObjectModel;
using System.Linq;
using LiteDB;

namespace MovieStore.Container
{
    public class ContainerCollection<T> : ObservableCollection<T>
        where T : ContainerBase
    {
        private LiteCollection<T> Repository;

        public ContainerCollection(LiteDatabase database, string collectionName)
            : base(database.GetCollection<T>(collectionName).FindAll())
        {
            Repository = database.GetCollection<T>(collectionName);
            Repository.EnsureIndex(x => x.Id);
        }

        public T FindById(int id)
        {
            return this.Where(x => x.Id == id).FirstOrDefault();
        }

        public void Update(T item)
        {
            item.DateUpdated = DateTime.Now;
            Repository.Update(item);
        }

        protected override void InsertItem(int index, T item)
        {
            if (!Contains(item))
                base.InsertItem(index, item);

            item.DateAdded = DateTime.Now;
            item.DateUpdated = DateTime.Now;

            Repository.Upsert(item);
        }

        protected override void RemoveItem(int index)
        {
            T item = this[index];
            base.RemoveItem(index);

            item.DateAdded = DateTime.MinValue;
            item.DateUpdated = DateTime.MinValue;

            Repository.Delete(item.Id);
        }
    }
}
