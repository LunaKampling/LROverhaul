using linerider.Game;
using System.Collections.Generic;

namespace linerider
{
    public class EditorCell : LineContainer<GameLine>
    {
        protected override LinkedListNode<GameLine> FindNodeAfter(LinkedListNode<GameLine> node, GameLine line)
        {
            if (line.ID < 0)
            {
                // Scenery lines want to skip right to the beginning of the
                // other scenery line ids
                while (node.Value.ID >= 0)
                {
                    node = node.Next;
                    if (node == null)
                        return null;
                }
            }
            while (line.ID >= 0
                ? line.ID < node.Value.ID // Physics
                : line.ID > node.Value.ID) // Scenery
            {
                node = node.Next;
                if (node == null)
                    return null;
            }
            return node;
        }
    }
}