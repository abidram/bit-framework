﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Foundation.DataAccess.Contracts;
using Foundation.Model.Contracts;
using System.Data.Entity;

namespace Bit.Data.EntityFramework.Implementations
{
    public class EfRepository<TEntity> : IRepository<TEntity>
        where TEntity : class, IEntity
    {
        private readonly DefaultDbContext _dbContext;
        private readonly DbSet<TEntity> _set;

        protected EfRepository()
        {
        }

        protected EfRepository(DefaultDbContext dbContext)
        {
            if (dbContext == null)
                throw new ArgumentNullException(nameof(dbContext));
            _dbContext = dbContext;
            _set = dbContext.Set<TEntity>();
        }

        public virtual async Task<TEntity> AddAsync(TEntity entityToAdd, CancellationToken cancellationToken)
        {
            if (entityToAdd == null)
                throw new ArgumentNullException(nameof(entityToAdd));

            _set.Add(entityToAdd);

            await SaveChangesAsync(cancellationToken);

            return entityToAdd;
        }

        public virtual async Task<IEnumerable<TEntity>> AddRangeAsync(IEnumerable<TEntity> entitiesToAdd, CancellationToken cancellationToken)
        {
            if (entitiesToAdd == null)
                throw new ArgumentNullException(nameof(entitiesToAdd));

            _set.AddRange(entitiesToAdd);

            await SaveChangesAsync(cancellationToken);

            return entitiesToAdd;
        }

        public virtual async Task<TEntity> UpdateAsync(TEntity entityToUpdate, CancellationToken cancellationToken)
        {
            if (entityToUpdate == null)
                throw new ArgumentNullException(nameof(entityToUpdate));

            _set.Attach(entityToUpdate);
            _dbContext.Entry(entityToUpdate).State = EntityState.Modified;

            await SaveChangesAsync(cancellationToken);

            return entityToUpdate;
        }

        public virtual async Task<TEntity> DeleteAsync(TEntity entityToDelete, CancellationToken cancellationToken)
        {
            if (entityToDelete == null)
                throw new ArgumentNullException(nameof(entityToDelete));

            if (entityToDelete is IArchivableEntity)
            {
                ((IArchivableEntity)entityToDelete).IsArchived = true;
                return await UpdateAsync(entityToDelete, cancellationToken);
            }
            else
            {
                _set.Attach(entityToDelete);
                _dbContext.Entry(entityToDelete).State = EntityState.Deleted;
                await SaveChangesAsync(cancellationToken);
                return entityToDelete;
            }
        }

        public virtual bool IsChangedProperty<TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> prop)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return _dbContext.Entry(entity).Property(prop).IsModified;
        }

        public virtual TProperty GetOriginalValue<TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> prop)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return _dbContext.Entry(entity).Property(prop).OriginalValue;
        }

        public virtual bool IsDeleted(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return _dbContext.Entry(entity).State == EntityState.Deleted;
        }

        public virtual bool IsAdded(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return _dbContext.Entry(entity).State == EntityState.Added;
        }

        public virtual bool IsModified(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return _dbContext.Entry(entity).State != EntityState.Unchanged;
        }

        public virtual void Detach(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _dbContext.Entry(entity).State = EntityState.Detached;
        }

        public virtual void Attach(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _set.Attach(entity);
        }

        public virtual TEntity Add(TEntity entityToAdd)
        {
            if (entityToAdd == null)
                throw new ArgumentNullException(nameof(entityToAdd));

            _set.Add(entityToAdd);

            SaveChanges();

            return entityToAdd;
        }

        public virtual IEnumerable<TEntity> AddRange(IEnumerable<TEntity> entitiesToAdd)
        {
            if (entitiesToAdd == null)
                throw new ArgumentNullException(nameof(entitiesToAdd));

            _set.AddRange(entitiesToAdd);

            SaveChanges();

            return entitiesToAdd;
        }

        public virtual TEntity Update(TEntity entityToUpdate)
        {
            if (entityToUpdate == null)
                throw new ArgumentNullException(nameof(entityToUpdate));

            _set.Attach(entityToUpdate);
            _dbContext.Entry(entityToUpdate).State = EntityState.Modified;

            SaveChanges();

            return entityToUpdate;
        }

        public virtual TEntity Delete(TEntity entityToDelete)
        {
            if (entityToDelete == null)
                throw new ArgumentNullException(nameof(entityToDelete));

            if (entityToDelete is IArchivableEntity)
            {
                ((IArchivableEntity)entityToDelete).IsArchived = true;
                return Update(entityToDelete);
            }
            else
            {
                _set.Attach(entityToDelete);
                _dbContext.Entry(entityToDelete).State = EntityState.Deleted;
                SaveChanges();
                return entityToDelete;
            }
        }

        public virtual IQueryable<TEntity> GetAll()
        {
            return _set.AsNoTracking();
        }

        public virtual async Task<IQueryable<TEntity>> GetAllAsync(CancellationToken cancellationToken)
        {
            return _set.AsNoTracking();
        }

        public virtual IQueryable<TChild> GetChildsQuery<TChild>(TEntity entity, Expression<Func<TEntity, ICollection<TChild>>> childs) where TChild : class
        {
            return _dbContext.Entry(entity).Collection(childs).Query();
        }

        public virtual async Task LoadChildsAsync<TProperty>(TEntity entity, Expression<Func<TEntity, ICollection<TProperty>>> childs, CancellationToken cancellationToken, bool forceReload = false) where TProperty : class
        {
            await _dbContext.Entry(entity).Collection(childs).LoadAsync(cancellationToken);
        }

        public virtual async Task LoadMemberAsync<TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> member, CancellationToken cancellationToken, bool forceReload = false) where TProperty : class
        {
            await _dbContext.Entry(entity).Reference(member).LoadAsync(cancellationToken);
        }

        public virtual async Task<bool> AnyChildAsync<TChild>(TEntity entity, Expression<Func<TEntity, ICollection<TChild>>> childs, Expression<Func<TChild, bool>> predicate, bool checkDatabase, CancellationToken cancellationToken) where TChild : class
        {
            return await _dbContext.Entry(entity).Collection(childs).Query().AnyAsync(predicate, cancellationToken);
        }

        public virtual bool AnyChild<TChild>(TEntity entity, Expression<Func<TEntity, ICollection<TChild>>> childs, Expression<Func<TChild, bool>> predicate, bool checkDatabase) where TChild : class
        {
            return _dbContext.Entry(entity).Collection(childs).Query().Any(predicate);
        }

        public virtual async Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            _dbContext.ChangeTracker.DetectChanges();
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public virtual void SaveChanges()
        {
            _dbContext.ChangeTracker.DetectChanges();
            _dbContext.SaveChanges();
        }
    }
}