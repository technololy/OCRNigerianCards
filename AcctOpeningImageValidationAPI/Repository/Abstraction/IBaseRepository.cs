using System;
using System.Collections.Generic;
using AcctOpeningImageValidationAPI.Models;

namespace AcctOpeningImageValidationAPI.Repository.Abstraction
{
    public interface IBaseRepository<T> where T : BaseEntity
    {
        /// <summary>
        /// Get All Entity Type 'T'
        /// </summary>
        /// <returns></returns>
        IEnumerable<T> GetAll();

        /// <summary>
        /// Find Method Using Predicate
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        IEnumerable<T> Find(Func<T, bool> predicate);

        /// <summary>
        /// Get Entity 'T' By Primary Key = Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        T GetById(int id);

        /// <summary>
        /// Create Entity Type 'T'
        /// </summary>
        /// <param name="entity"></param>
        void Create(T entity);

        /// <summary>
        /// Create Multiple Entity List<typeparamref name="T"/>
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        List<T> CreateMultiple(List<T> entity);

        /// <summary>
        /// Create And Return Entity Type 'T'
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        T CreateAndReturn(T entity);

        /// <summary>
        /// Update Entity Type 'T'
        /// </summary>
        /// <param name="entity"></param>
        T Update(T entity);

        /// <summary>
        /// Delete Entity Type 'T'
        /// </summary>
        /// <param name="entity"></param>
        void Delete(T entity);

        /// <summary>
        /// Count Entities Type 'T'
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        int Count(Func<T, bool> predicate);

        /// <summary>
        /// Convert Entity To List
        /// </summary>
        /// <returns></returns>
        List<T> ToList();
    }
}
