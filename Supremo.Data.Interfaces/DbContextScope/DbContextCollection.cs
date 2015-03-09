using Supremo.Data.Interfaces.Interfaces.DbContextScope;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Supremo.Data.Interfaces.DbContextScope
{
    public class DbContextCollection : IDbContextCollection
    {
        Dictionary<Type, DbContext> _initializedDbContexts;
        Dictionary<DbContext, DbContextTransaction> _transactions;
        IsolationLevel? _isolationLevel;
        readonly IDbContextFactory _dbContextFactory;
        bool _disposed, _completed, _readOnly;

        internal Dictionary<Type, DbContext> InitializedDbContexts { get { return _initializedDbContexts; } }

        public DbContextCollection(bool readOnly = false, IsolationLevel? isolationLevel = null, IDbContextFactory dbContextFactory = null)
        {
            _disposed = _completed = false;
            _initializedDbContexts = new Dictionary<Type, DbContext>();
            _transactions = new Dictionary<DbContext, DbContextTransaction>();

            _readOnly = readOnly;
            _isolationLevel = isolationLevel;
            _dbContextFactory = dbContextFactory;
        }

        public TDbContext Get<TDbContext>() where TDbContext : DbContext
        {
            if (_disposed)
                throw new ObjectDisposedException("DbContextCollection");

            var requestedType = typeof(TDbContext);

            if(!_initializedDbContexts.ContainsKey(requestedType))
                CreateContextWithTransactionIfNeeded<TDbContext>(requestedType);

            return _initializedDbContexts[requestedType] as TDbContext;
        }

        private void CreateContextWithTransactionIfNeeded<TDbContext>(Type reqestedType) where TDbContext : DbContext
        {
            var dbContext = _dbContextFactory != null ? _dbContextFactory.CreateDbContext<TDbContext>() : Activator.CreateInstance<TDbContext>();
            _initializedDbContexts.Add(reqestedType, dbContext);

            if (_readOnly)
                dbContext.Configuration.AutoDetectChangesEnabled = false;

            if(_isolationLevel.HasValue)
            {
                var tran = dbContext.Database.BeginTransaction(_isolationLevel.Value);
                _transactions.Add(dbContext, tran);
            }
        }

        public int Commit()
        {
            if (_disposed)
                throw new ObjectDisposedException("DbContextCollection");
            if (_completed)
                throw new InvalidOperationException("You can't call Commit() or Rollback() more than once on a DbContextCollection. All the changes in the DbContext instances managed by this collection have already been saved or rollback and all database transactions have been completed and closed. If you wish to make more data changes, create a new DbContextCollection and make your changes there.");

            ExceptionDispatchInfo lastError = null;
            int operationCount = 0;

            foreach (var dbContext in _initializedDbContexts.Values)
            {
                try
                {
                    if (!_readOnly)
                        operationCount += dbContext.SaveChanges();

                    if(_transactions.ContainsKey(dbContext))
                    {
                        var tran = _transactions[dbContext];
                        tran.Commit();
                        tran.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    lastError = ExceptionDispatchInfo.Capture(ex);
                }
            }
            _transactions.Clear();
            _completed = true;

            if (lastError != null)
                lastError.Throw();

            return operationCount;
        }

        public Task<int> CommitAsync()
        {
            return CommitAsync(CancellationToken.None);
        }

        public async Task<int> CommitAsync(CancellationToken cancelToken)
        {
            if (cancelToken == null)
                throw new ArgumentNullException("cancelToken");
            if (_disposed)
                throw new ObjectDisposedException("DbContextCollection");
            if (_completed)
                throw new InvalidOperationException("You can't call Commit() or Rollback() more than once on a DbContextCollection. All the changes in the DbContext instances managed by this collection have already been saved or rollback and all database transactions have been completed and closed. If you wish to make more data changes, create a new DbContextCollection and make your changes there.");


            ExceptionDispatchInfo lastError = null;

            int operationCount = 0;

            foreach (var dbContext in _initializedDbContexts.Values)
            {
                try
                {
                    if (!_readOnly)
                        operationCount += await dbContext.SaveChangesAsync(cancelToken).ConfigureAwait(false);


                    if (_transactions.ContainsKey(dbContext))
                    {
                        var tran = _transactions[dbContext];
                        tran.Commit();
                        tran.Dispose();
                    }
                }
                catch (Exception e)
                {
                    lastError = ExceptionDispatchInfo.Capture(e);
                }
            }

            _transactions.Clear();
            _completed = true;

            if (lastError != null)
                lastError.Throw();

            return operationCount;
        }

        public void Rollback()
        {
            if (_disposed)
                throw new ObjectDisposedException("DbContextCollection");
            if (_completed)
                throw new InvalidOperationException("You can't call Commit() or Rollback() more than once on a DbContextCollection. All the changes in the DbContext instances managed by this collection have already been saved or rollback and all database transactions have been completed and closed. If you wish to make more data changes, create a new DbContextCollection and make your changes there.");

            ExceptionDispatchInfo lastError = null;

            foreach (var dbContext in _initializedDbContexts.Values)
            {
                try
                {
                    if (_transactions.ContainsKey(dbContext))
                    {
                        var tran = _transactions[dbContext];
                        tran.Rollback();
                        tran.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    lastError = ExceptionDispatchInfo.Capture(ex);
                }
            }
            _transactions.Clear();
            _completed = true;

            if (lastError != null)
                lastError.Throw();
        }


        public void Dispose()
        {
            if (_disposed)
                return;

            if (!_completed)
            {
                try
                {
                    if (_readOnly) Commit();
                    else Rollback();
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e);
                }
            }

            foreach (var dbContext in _initializedDbContexts.Values)
            {
                try
                {
                    dbContext.Dispose();
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e);
                }
            }

            _initializedDbContexts.Clear();
            _disposed = true;
        }
    }
}
