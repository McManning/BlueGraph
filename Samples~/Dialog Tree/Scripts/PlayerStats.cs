using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;

namespace BlueGraphSamples
{
    public enum PlayerStat
    {
        Survival,
        Persuasion,
        Education,
        ChanceToCrit,
        Intimidation,
        AnimalHandling,
        Luck
    }

    /// <summary>
    /// S.P.E.C.I.A.L stats tracking for the player
    /// </summary>
    public class PlayerStats : MonoBehaviour
    {
        private readonly Dictionary<PlayerStat, int> stats = new Dictionary<PlayerStat, int>();

        void Start()
        {
            var values = transform.Find("Values");

            for (int i = 0; i < values.childCount; i++)
            {
                var el = values.GetChild(i);
                var stat = (PlayerStat)Enum.Parse(typeof(PlayerStat), el.name);
                var label = el.GetComponent<Text>();
                
                stats[stat] = 0;

                // Map +/- buttons to modify the stat
                el.Find("Add").GetComponent<Button>().onClick.AddListener(() =>
                {
                    stats[stat] += 1;
                    label.text = stats[stat].ToString();
                });

                el.Find("Subtract").GetComponent<Button>().onClick.AddListener(() =>
                {
                    stats[stat] -= 1;
                    label.text = stats[stat].ToString();
                });
            }
        }
        
        public int GetStat(PlayerStat stat)
        {
            return stats[stat];
        }
    }
}
