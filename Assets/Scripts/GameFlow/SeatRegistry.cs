using System.Collections.Generic;
using UnityEngine;

public class SeatRegistry : MonoBehaviour
{
    [System.Serializable]
    public class SeatEntry
    {
        public SeatId Id;
        public TeamId Team;
        public bool IsLocal;
        public HandController Hand;
    }

    [Header("Configure seats here (scene)")]
    public List<SeatEntry> Seats = new();

    private readonly Dictionary<SeatId, SeatContext> _ctx = new();
    private bool _built;

    public bool IsBuilt => _built;

    public void Build(IAgentFactory agentFactory)
    {
        _ctx.Clear();
        foreach (var s in Seats)
        {
            var c = new SeatContext
            {
                Id = s.Id,
                Team = s.Team,
                IsLocal = s.IsLocal,
                Hand = s.Hand
            };
            c.Agent = agentFactory.Create(c);
            _ctx[c.Id] = c;
        }
                _built = true;

    }

    public SeatContext Get(SeatId id) => _ctx[id];
    public IEnumerable<SeatContext> All() => _ctx.Values;

    public static SeatId Next(SeatId s) => (SeatId)(((int)s + 1) % 4);

    public List<SeatId> OrderAfter(SeatId dealer)
    {
        var list = new List<SeatId>();
        var cur = Next(dealer);
        for (int i = 0; i < 4; i++) { list.Add(cur); cur = Next(cur); }
        return list;
    }


        // ...



}
