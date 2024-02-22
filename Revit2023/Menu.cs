using System;
using System.Collections.Generic;
using Ara3D.Utils;

namespace Ara3D.Bowerbird.Wpf
{
    public class MenuItem
    {
        public string Header { get; }
        public INamedCommand Command { get; }
        public IReadOnlyList<MenuItem> Items { get; }

        public MenuItem(string header, INamedCommand command, params MenuItem[] items)
        {
            Header = header;
            Command = command;
            Items = items;
        }
    }

    public class MenuPath
    {
        public IReadOnlyList<string> Parts { get; }

        public MenuPath(IReadOnlyList<string> parts)
            => Parts = parts;

        public static MenuPath FromParts(string path)
            => new MenuPath(path.Split('|'));
        
        public static MenuPath Create(MenuPath prev, string path)
        {
            var prevParts = prev.Parts;
            var currParts = path.Split('|');
            var newParts = new List<string>();
            for (var i = 0; i < currParts.Length; i++)
            {
                var part = currParts[i];
                if (string.IsNullOrEmpty(part))
                {
                    if (i >= prevParts.Count)
                        throw new Exception("Empty part requires non-empty part in previous");
                    var prevPart = prevParts[i];
                    newParts.Add(prevPart);
                }
                else
                {
                    newParts.Add(part);
                }
            }
            return new MenuPath(newParts);
        }
    }

    // WIP: 
    // One of the ideas is that we can define a menu declarative with a list of string pairs.
    // First string is the menu path, and the second is the command associated with it. 
    public class Menu
    {
        public IReadOnlyList<(string, string)> MenuToCommand { get; }

        public Menu(IReadOnlyList<(string, string)> menuToCommands, IDictionary<string, INamedCommand> lookup)
        {
            var prevMenuItem = new List<string>();
            var list = new List<(MenuPath, INamedCommand)>();
            foreach (var pair in menuToCommands)
            {
                var command = lookup[pair.Item2];
                
            }
        }
     }
}
