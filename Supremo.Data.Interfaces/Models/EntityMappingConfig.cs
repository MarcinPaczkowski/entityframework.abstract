using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Supremo.Data.Interfaces.Models
{
    public class EntityMappingConfig
    {
        public static void CreateMappings(DbModelBuilder modelBuilder, List<AssemblyName> assemblyNames)
        {
            foreach (var name in assemblyNames)
            {
                var entityTypes = Assembly.Load(name).GetTypes().Where(t => Attribute.GetCustomAttributes(t).ToList().Exists(a => a is EntityMapping));
                foreach (var entityType in entityTypes)
                {
                    entityType.GetMethod("CreateEntityMapping", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { modelBuilder });
                }
            }            
        }
    }
}
