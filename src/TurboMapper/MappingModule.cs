using System;
using System.Linq;

namespace TurboMapper
{
    public abstract class MappingModule<TSource, TTarget> : IMappingModule
    {
        private readonly Action<MappingExpression<TSource, TTarget>> _configAction;
        private readonly bool _enableDefaultMapping;

        public MappingModule(bool enableDefaultMapping = true)
        {
            _configAction = CreateMappings();
            _enableDefaultMapping = enableDefaultMapping;
        }

        void IMappingModule.CreateMap(IObjectMap mapper)
        {
            var expression = new MappingExpression<TSource, TTarget>();
            _configAction(expression);

            // Add default mappings for unmapped properties if enabled
            if (_enableDefaultMapping)
            {
                var sourceProps = typeof(TSource).GetProperties();
                var targetProps = typeof(TTarget).GetProperties();

                foreach (var sourceProp in sourceProps)
                {
                    // Check if this property is already mapped
                    var isMapped = expression.Mappings.Exists(m =>
                        m.SourcePropertyPath.Split('.').Last() == sourceProp.Name);

                    if (!isMapped)
                    {
                        var targetProp = targetProps.FirstOrDefault(p =>
                            p.Name == sourceProp.Name &&
                            p.CanWrite);

                        if (targetProp != null)
                            // Add default mapping for unmapped properties
                            expression.Mappings.Add(new PropertyMapping
                            {
                                SourceProperty = sourceProp.Name,
                                TargetProperty = targetProp.Name,
                                SourcePropertyPath = sourceProp.Name,
                                TargetPropertyPath = targetProp.Name
                            });
                    }
                }
            }

            mapper.CreateMap<TSource, TTarget>(expression.Mappings, _enableDefaultMapping);
        }

        public abstract Action<IMappingExpression<TSource, TTarget>> CreateMappings();
    }
}