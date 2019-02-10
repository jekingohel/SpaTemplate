using System;
using System.Collections.Generic;

namespace SpaTemplate.Core
{
    public interface IRepository : IUnityOfWork
    {
        T GetById<T>(Guid id) where T : BaseEntity;
        List<T> List<T>() where T : BaseEntity;
        T Add<T>(T entity) where T : BaseEntity;
        void Update<T>(T entity) where T : BaseEntity;
        void Delete<T>(T entity) where T : BaseEntity;
        bool EntityExists<T>(Guid entity) where T : BaseEntity;
        void UpdateEntity<T>(T entity) where T : BaseEntity;
    }
}
