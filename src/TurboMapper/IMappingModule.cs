namespace TurboMapper
{
    /// <summary>
    /// Defines a module for configuring object mappings.
    /// </summary>
    internal interface IMappingModule
    {
        /// <summary>
        /// Configures mappings using the provided IObjectMap instance.
        /// </summary>
        /// <param name="mapper"></param>
        void CreateMap(IObjectMap mapper);
    }
}