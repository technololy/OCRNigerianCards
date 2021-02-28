using System;
using System.Collections.Generic;
using System.Linq;
using AcctOpeningImageValidationAPI.Models;
using AcctOpeningImageValidationAPI.Repository.Abstraction;
using Microsoft.EntityFrameworkCore;

namespace AcctOpeningImageValidationAPI.Repository
{
    /// <summary>
    /// Concrete Implementation Of IBaseRepository
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BaseRepository<T> : IBaseRepository<T> where T : BaseEntity
    {
        protected readonly SterlingOnebankIDCardsContext _context;
        /// <summary>
        /// Constructor, This Constructor Sub-Method Parent Constructor To
        /// Initialize Application DbContext
        /// Also This Generic Method, Extends The Basic CRUD Stubs To Other Repository
        /// </summary>
        /// <param name="context"></param>
        public BaseRepository(SterlingOnebankIDCardsContext context)
        {
            _context = context;
        }

        /// <summary>
        ///  SaveAsync Method For Saving Type T where : BaseEntity
        /// </summary>
        protected void SaveAsync() => _context.SaveChangesAsync();

        /// <summary>
        /// Save Method For Entity
        /// </summary>
        protected void Save() => _context.SaveChanges();

        /// <summary>
        /// Abstraction Method To Give Count Of Entitys
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public int Count(Func<T, bool> predicate)
        {
            return _context.Set<T>().Where(predicate).Count();
        }

        /// <summary>
        /// Method To Create Record
        /// </summary>
        /// <param name="entity"></param>
        public void Create(T entity)
        {
            entity.DateModified = DateTime.Now;
            entity.IsEnabled = true;
            _context.Add(entity);
            Save();
        }

        /// <summary>
        /// Method To Create Record And Return At The Same Time
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public T CreateAndReturn(T entity)
        {
            Create(entity);

            return entity;
        }

        /// <summary>
        /// This Method Deletes Entity 'T'
        /// </summary>
        /// <param name="entity"></param>
        public void Delete(T entity)
        {
            _context.Remove(entity);

            Save();
        }

        /// <summary>
        /// Find Entity By Argument Predicate
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public IEnumerable<T> Find(Func<T, bool> predicate)
        {
            return _context.Set<T>().Where(predicate);
        }

        /// <summary>
        /// Get All  Values For Entity
        /// </summary>
        /// <returns></returns>
        public IEnumerable<T> GetAll()
        {
            return _context.Set<T>();
        }

        /// <summary>
        /// Get Entity By Its Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public T GetById(int id)
        {
            return _context.Set<T>().Find(id);
        }

        /// <summary>
        /// Convert To List<T>
        /// </summary>
        /// <returns></returns>
        public List<T> ToList()
        {
            return _context.Set<T>().ToList<T>();
        }

        /// <summary>
        /// Update Entity Object
        /// </summary>
        /// <param name="entity"></param>
        public T Update(T entity)
        {
            entity.DateModified = DateTime.Now;
            _context.Entry(entity).State = EntityState.Modified;

            Save();

            return entity;
        }

        /// <summary>
        /// Create Multiple
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public List<T> CreateMultiple(List<T> entity)
        {
            _context.Set<T>().AddRange(entity);
            Save();
            return entity;
        }
    }
}
