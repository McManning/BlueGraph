using UnityEngine;
using BlueGraph;

namespace BlueGraphSamples
{
    [Node("Get S.P.E.C.I.A.L.", Path = "Player")]
    [Output("", typeof(int))]
    [Tags("Player")]
    public class GetPlayerStat : Node
    {
        [Input] public PlayerStat stat;

        public override object OnRequestValue(Port port)
        {
            var stat = GetInputValue("stat", this.stat);

            var stats = GameObject.Find("SPECIAL").GetComponent<PlayerStats>();
            return stats.GetStat(stat);
        }
    }
}
