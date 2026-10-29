namespace Cthangover.Tools.Schemas
{
    using Cthangover.Core.UI.Tool;
    using Godot;

    /// <summary>Puts the command palette into the tools menu.</summary>
    public class PaletteToolProvider : IToolProvider
    {
        /// <inheritdoc />
        public string Id => "schema_palette";

        /// <inheritdoc />
        public string LocaleKey => "tools/schema/palette/title";

        /// <inheritdoc />
        public Window CreateWindow() => new CommandPaletteWindow();
    }
}
