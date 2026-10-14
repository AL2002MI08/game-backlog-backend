using GameBacklog.Api.Dtos;
using GameBacklog.Api.Exceptions;
using GameBacklog.Domain.Entities;

namespace GameBacklog.Api.Extensions
{
    public static class ObjectiveListExtensions
    {
        public static List<Objective> ToEntities(this IEnumerable<CreateObjectiveRequest> requests, Guid gameId, DateTime now)
        {
            var list = requests.ToList();
            var labels = list.Select(o => o.Label?.Trim()).ToList();

            if (labels.Any(string.IsNullOrEmpty))
                throw new BadRequestException("Objective label cannot be empty.");

            if (labels.Distinct(StringComparer.OrdinalIgnoreCase).Count() < labels.Count)
                throw new BadRequestException("Objective labels within a game must be unique.");

            var objectives = new List<Objective>();
            foreach (var req in list)
            {
                var objective = new Objective
                {
                    Id = Guid.NewGuid(),
                    GameId = gameId,
                    Label = req.Label.Trim(),
                    Position = objectives.Count + 1,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                objectives.Add(objective);
                if (req.Position is not null)
                    objectives.Reorder(objective, req.Position.Value, now);
            }
            return objectives;
        }

        public static void Reorder(this List<Objective> list, Objective item, int targetPosition, DateTime now)
        {
            list.Remove(item);
            var targetIndex = Math.Clamp(targetPosition - 1, 0, list.Count);
            list.Insert(targetIndex, item);
            list.RenumberPositions(now);
        }

        public static void RenumberPositions(this List<Objective> list, DateTime now)
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[i].Position = i + 1;
                list[i].UpdatedAt = now;
            }
        }
    }
}
