namespace Services.Broadcast.Entities.Enums
{
    /// <summary>
    /// Indicates the type of processor.
    /// </summary>
    public enum InventoryProgramsProcessorType
    {
        /// <summary>
        /// Processing is scoped to a file's inventory.
        /// </summary>
        ByFile,

        /// <summary>
        /// Processing is scoped by the source.
        /// </summary>
        BySource
    }
}