using Supremo.Data.Interfaces.Enums;
using Supremo.Data.Interfaces.Interfaces.DbContextScope;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Supremo.Data.Interfaces.DbContextScope
{
    public class DbContextScope : IDbContextScope
    {
        private bool _disposed, _readOnly, _completed, _nested;
        DbContextScope _parentScope;
        DbContextCollection _dbContexts;

        internal InstanceIdentifier InstanceIdentifier = new InstanceIdentifier();
        public IDbContextCollection DbContexts { get { return _dbContexts; } }

        public DbContextScope(IDbContextFactory dbContextFactory = null) :
            this(joiningOption: DbContextScopeOption.JoinExisting, readOnly: false, isolationLevel: null, dbContextFactory: dbContextFactory)
        {}

        public DbContextScope(bool readOnly, IDbContextFactory dbContextFactory = null)
            : this(joiningOption: DbContextScopeOption.JoinExisting, readOnly: readOnly, isolationLevel: null, dbContextFactory: dbContextFactory)
        {}

        public DbContextScope(DbContextScopeOption joiningOption, bool readOnly, IsolationLevel? isolationLevel, IDbContextFactory dbContextFactory = null)
        {
            if (isolationLevel.HasValue && joiningOption == DbContextScopeOption.JoinExisting)
                throw new ArgumentException("Cannot join an ambient DbContextScope when an explicit database transaction is required. When requiring explicit database transactions to be used (i.e. when the 'isolationLevel' parameter is set), you must not also ask to join the ambient context (i.e. the 'joinAmbient' parameter must be set to false).");

            _disposed = false;
            _completed = false;
            _readOnly = readOnly;
            _parentScope = AmbientScope.GetAmbientScope();
            if(_parentScope != null && joiningOption == DbContextScopeOption.JoinExisting)
            {
                if(_parentScope._readOnly && !this._readOnly)
                    throw new InvalidOperationException("Cannot nest a read/write DbContextScope within a read-only DbContextScope.");
                _nested = true;
                _dbContexts = _parentScope._dbContexts;
            }
            else
            {
                _nested = false;
                _dbContexts = new DbContextCollection(readOnly, isolationLevel, dbContextFactory);
            }

            AmbientScope.SetAmbientScope(this);
        }

        public int SaveChanges()
        {
            if (_disposed)
                throw new ObjectDisposedException("DbContextScope");
            if (_completed)
                throw new InvalidOperationException("You cannot call SaveChanges() more than once on a DbContextScope. A DbContextScope is meant to encapsulate a business transaction: create the scope at the start of the business transaction and then call SaveChanges() at the end. Calling SaveChanges() mid-way through a business transaction doesn't make sense and most likely mean that you should refactor your service method into two separate service method that each create their own DbContextScope and each implement a single business transaction.");

            var operationCount = 0;
            if (!_nested)
                operationCount = CommitInternals();

            _completed = true;
            return operationCount;
        }

        public Task<int> SaveChangesAsync()
        {
            return SaveChangesAsync(CancellationToken.None);
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancelToken)
        {
            if (cancelToken == null)
                throw new ArgumentNullException("cancelToken");
            if (_disposed)
                throw new ObjectDisposedException("DbContextScope");
            if (_completed)
                throw new InvalidOperationException("You cannot call SaveChanges() more than once on a DbContextScope. A DbContextScope is meant to encapsulate a business transaction: create the scope at the start of the business transaction and then call SaveChanges() at the end. Calling SaveChanges() mid-way through a business transaction doesn't make sense and most likely mean that you should refactor your service method into two separate service method that each create their own DbContextScope and each implement a single business transaction.");

            var operationCount = 0;
            if (!_nested)
            {
                operationCount = await CommitInternalAsync(cancelToken).ConfigureAwait(false);
            }

            _completed = true;
            return operationCount;
        }

        private int CommitInternals()
        {
            return _dbContexts.Commit();
        }

        private Task<int> CommitInternalAsync(CancellationToken cancelToken)
        {
            return _dbContexts.CommitAsync(cancelToken);
        }

        private void RollbackInternal()
        {
            _dbContexts.Rollback();
        }

        public void RefreshEntitiesInParentScope(IEnumerable entities)
        {
            if (entities == null)
                return;

            if (_parentScope == null)
                return;

            if (_nested)
                return;

            foreach (IObjectContextAdapter contextInCurrentScope in _dbContexts.InitializedDbContexts.Values)
            {
                var correspondingParentContext =
                    _parentScope._dbContexts.InitializedDbContexts.Values.SingleOrDefault(parentContext => parentContext.GetType() == contextInCurrentScope.GetType())
                    as IObjectContextAdapter;

                if (correspondingParentContext == null)
                    continue; // No DbContext of this type has been created in the parent scope yet. So no need to refresh anything for this DbContext type.

                // Both our scope and the parent scope have an instance of the same DbContext type. 
                // We can now look in the parent DbContext instance for entities that need to
                // be refreshed.
                foreach (var toRefresh in entities)
                {
                    // First, we need to find what the EntityKey for this entity is. 
                    // We need this EntityKey in order to check if this entity has
                    // already been loaded in the parent DbContext's first-level cache (the ObjectStateManager).
                    ObjectStateEntry stateInCurrentScope;
                    if (contextInCurrentScope.ObjectContext.ObjectStateManager.TryGetObjectStateEntry(toRefresh, out stateInCurrentScope))
                    {
                        var key = stateInCurrentScope.EntityKey;

                        // Now we can see if that entity exists in the parent DbContext instance and refresh it.
                        ObjectStateEntry stateInParentScope;
                        if (correspondingParentContext.ObjectContext.ObjectStateManager.TryGetObjectStateEntry(key, out stateInParentScope))
                        {
                            // Only refresh the entity in the parent DbContext from the database if that entity hasn't already been
                            // modified in the parent. Otherwise, let the whatever concurency rules the application uses
                            // apply.
                            if (stateInParentScope.State == EntityState.Unchanged)
                            {
                                correspondingParentContext.ObjectContext.Refresh(RefreshMode.StoreWins, stateInParentScope.Entity);
                            }
                        }
                    }
                }
            }
        }

        public async Task RefreshEntitiesInParentScopeAsync(IEnumerable entities)
        {
            if (entities == null)
                return;

            if (_parentScope == null)
                return;

            if (_nested)
                return;

            foreach (IObjectContextAdapter contextInCurrentScope in _dbContexts.InitializedDbContexts.Values)
            {
                var correspondingParentContext =
                    _parentScope._dbContexts.InitializedDbContexts.Values.SingleOrDefault(parentContext => parentContext.GetType() == contextInCurrentScope.GetType())
                    as IObjectContextAdapter;

                if (correspondingParentContext == null)
                    continue;

                foreach (var toRefresh in entities)
                {
                    ObjectStateEntry stateInCurrentScope;
                    if (contextInCurrentScope.ObjectContext.ObjectStateManager.TryGetObjectStateEntry(toRefresh, out stateInCurrentScope))
                    {
                        var key = stateInCurrentScope.EntityKey;

                        ObjectStateEntry stateInParentScope;
                        if (correspondingParentContext.ObjectContext.ObjectStateManager.TryGetObjectStateEntry(key, out stateInParentScope))
                        {
                            if (stateInParentScope.State == EntityState.Unchanged)
                            {
                                await correspondingParentContext.ObjectContext.RefreshAsync(RefreshMode.StoreWins, stateInParentScope.Entity).ConfigureAwait(false);
                            }
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            if (!_nested)
            {
                if (!_completed)                
                {                    
                    try
                    {
                        if (_readOnly)            
                            CommitInternals();
                        else          
                            RollbackInternal();
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine(e);
                    }

                    _completed = true;
                }

                _dbContexts.Dispose();
            }

            // Pop ourself from the ambient scope stack
            var currentAmbientScope = AmbientScope.GetAmbientScope();
            if (currentAmbientScope != this) // This is a serious programming error. Worth throwing here.
                throw new InvalidOperationException("DbContextScope instances must be disposed of in the order in which they were created!");

            AmbientScope.RemoveAmbientScope();

            if (_parentScope != null)
            {
                if (_parentScope._disposed)
                {

                    var message = @"PROGRAMMING ERROR - When attempting to dispose a DbContextScope, we found that our parent DbContextScope has already been disposed! This means that someone started a parallel flow of execution (e.g. created a TPL task, created a thread or enqueued a work item on the ThreadPool) within the context of a DbContextScope without suppressing the ambient context first. 

In order to fix this:
1) Look at the stack trace below - this is the stack trace of the parallel task in question.
2) Find out where this parallel task was created.
3) Change the code so that the ambient context is suppressed before the parallel task is created. You can do this with IDbContextScopeFactory.SuppressAmbientContext() (wrap the parallel task creation code block in this). 

Stack Trace:
" + Environment.StackTrace;

                    System.Diagnostics.Debug.WriteLine(message);
                }
                else
                {
                    AmbientScope.SetAmbientScope(_parentScope);
                }
            }

            _disposed = true;

        }
    }
}
