using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace Supremo.Data.Interfaces.DbContextScope
{
    public class AmbientScope
    {
        private static readonly string AmbientDbContextScopeKey = "AmbientDbcontext_" + Guid.NewGuid();
        private static readonly ConditionalWeakTable<InstanceIdentifier, DbContextScope> DbContextScopeInstances = new ConditionalWeakTable<InstanceIdentifier, DbContextScope>();
        
        internal static void SetAmbientScope(DbContextScope newAmbientScope)
        {
            if (newAmbientScope == null)
                throw new ArgumentNullException("newAmbientScope");

            var current = CallContext.LogicalGetData(AmbientDbContextScopeKey) as InstanceIdentifier;

            if (current == newAmbientScope.InstanceIdentifier)
                return;

            // Store the new scope's instance identifier in the CallContext, making it the ambient scope
            CallContext.LogicalSetData(AmbientDbContextScopeKey, newAmbientScope.InstanceIdentifier);

            // Keep track of this instance (or do nothing if we're already tracking it)
            DbContextScopeInstances.GetValue(newAmbientScope.InstanceIdentifier, key => newAmbientScope);
        }

        internal static void RemoveAmbientScope()
        {
            var current = CallContext.LogicalGetData(AmbientDbContextScopeKey) as InstanceIdentifier;
            CallContext.LogicalSetData(AmbientDbContextScopeKey, null);

            // If there was an ambient scope, we can stop tracking it now
            if (current != null)
            {
                DbContextScopeInstances.Remove(current);
            }
        }

        internal static void HideAmbientScope()
        {
            CallContext.LogicalSetData(AmbientDbContextScopeKey, null);
        }

        internal static DbContextScope GetAmbientScope()
        {
            // Retrieve the identifier of the ambient scope (if any)
            var instanceIdentifier = CallContext.LogicalGetData(AmbientDbContextScopeKey) as InstanceIdentifier;
            if (instanceIdentifier == null)
                return null; // Either no ambient context has been set or we've crossed an app domain boundary and have (intentionally) lost the ambient context

            // Retrieve the DbContextScope instance corresponding to this identifier
            DbContextScope ambientScope;
            if (DbContextScopeInstances.TryGetValue(instanceIdentifier, out ambientScope))
                return ambientScope;

            // We have an instance identifier in the CallContext but no corresponding instance
            // in our DbContextScopeInstances table. This should never happen! The only place where
            // we remove the instance from the DbContextScopeInstances table is in RemoveAmbientScope(),
            // which also removes the instance identifier from the CallContext. 
            //
            // There's only one scenario where this could happen: someone let go of a DbContextScope 
            // instance without disposing it. In that case, the CallContext
            // would still contain a reference to the scope and we'd still have that scope's instance
            // in our DbContextScopeInstances table. But since we use a ConditionalWeakTable to store 
            // our DbContextScope instances and are therefore only holding a weak reference to these instances, 
            // the GC would be able to collect it. Once collected by the GC, our ConditionalWeakTable will return
            // null when queried for that instance. In that case, we're OK. This is a programming error 
            // but our use of a ConditionalWeakTable prevented a leak.
            System.Diagnostics.Debug.WriteLine("Programming error detected. Found a reference to an ambient DbContextScope in the CallContext but didn't have an instance for it in our DbContextScopeInstances table. This most likely means that this DbContextScope instance wasn't disposed of properly. DbContextScope instance must always be disposed. Review the code for any DbContextScope instance used outside of a 'using' block and fix it so that all DbContextScope instances are disposed of.");
            return null;
        }
    }
}
