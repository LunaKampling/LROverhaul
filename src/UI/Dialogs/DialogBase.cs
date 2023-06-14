using Gwen.Controls;

namespace linerider.UI
{
    public abstract class DialogBase : WindowControl
    {
        protected GameCanvas _canvas;
        protected Editor _editor;
        public DialogBase(GameCanvas parent, Editor editor) : base(parent)
        {
            DeleteOnClose = true;
            _canvas = parent;
            _editor = editor;
        }
    }
}
