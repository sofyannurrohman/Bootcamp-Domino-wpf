using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
namespace DominoGame.Models
{       public class Player
        {
            public string Name { get; }
            public List<DominoTile> Hand { get; } = new();

            public Player(string name)
            {
                Name = name;
            }

            public bool HasPlayableTile(int leftEnd, int rightEnd)
            {
                foreach (var tile in Hand)
                {
                    if (tile.Matches(leftEnd) || tile.Matches(rightEnd))
                        return true;
                }
                return false;
            }
        }
    }


