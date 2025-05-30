using UnityEngine;

namespace OkapiKit
{
    [AddComponentMenu("Okapi/Trigger/On Grid Event")]
    public class TriggerOnGridEvent : Trigger
    {
        public enum GridEvent { HitWall, PushObject, HitObject, StepEnd, RotateEnd, WasHit, WasPushed };

        [SerializeField]
        private GridEvent eventType;
        [SerializeField]
        private Hypertag[] tags;

        public override string GetTriggerTitle() { return "On Grid Event"; }

        public override string GetRawDescription(string ident, GameObject refObject)
        {
            var desc = "";
            switch (eventType)
            {
                case GridEvent.HitWall:
                    desc = "When this object hits a grid wall ";
                    break;
                case GridEvent.PushObject:
                    desc = "When this object pushes another ";
                    break;
                case GridEvent.HitObject:
                    desc = "When this objects hits another object ";
                    break;
                case GridEvent.StepEnd:
                    desc = "When this object finishes a movement step ";
                    break;
                case GridEvent.RotateEnd:
                    desc = "When this object finishes a rotation step ";
                    break;
                case GridEvent.WasHit:
                    desc = "When this object was hit by another object ";
                    break;
                case GridEvent.WasPushed:
                    desc = "When this object was pushed by another object ";
                    break;
                default:
                    break;
            }

            if ((eventType == GridEvent.PushObject) || (eventType == GridEvent.HitObject) ||
                (eventType == GridEvent.WasPushed) || (eventType == GridEvent.WasHit))
            {
                if ((tags != null) && (tags.Length > 0))
                {
                    desc += "with tags [";
                    for (int i = 0; i < tags.Length; i++)
                    {
                        if (tags[i] == null) desc += "NULL";
                        else desc += tags[i].name;
                        if (i < tags.Length - 1) desc += ",";
                    }
                    desc += "] ";
                }
            }

            return desc;
        }

        protected override void CheckErrors()
        {
            base.CheckErrors();

            if ((eventType == GridEvent.PushObject) || (eventType == GridEvent.HitObject) ||
                (eventType == GridEvent.WasPushed) || (eventType == GridEvent.WasHit))
            {
                if ((tags == null) || (tags.Length == 0))
                {
                    _logs.Add(new LogEntry(LogEntry.Type.Error, $"No tags defined - {eventType} only detects objects with the given tags!", $"{eventType} needs to filter what objects interactions are valid, and it uses the tag list to do so.\nAdd on the Tags list what objects are allowed to interact with this object."));
                }
                else
                {
                    int index = 0;
                    foreach (var tag in tags)
                    {
                        if (tag == null)
                        {
                            _logs.Add(new LogEntry(LogEntry.Type.Error, $"Empty tags slot {index}!", "An empty slot doesn't do anything, clean up after yourself! :)"));
                        }
                        index++;
                    }
                }
            }
        }

        internal void ThrowEvent(GridEvent evt, GridObject otherObject = null)
        {
            if (!isTriggerEnabled) return;
            if (!EvaluatePreconditions()) return;

            if (evt != eventType) return;

            switch (eventType)
            {
                case GridEvent.PushObject:
                case GridEvent.HitObject:
                case GridEvent.WasHit:
                case GridEvent.WasPushed:
                    if (otherObject == null) return;
                    if (!otherObject.HasHypertags(tags)) return;
                    break;
                case GridEvent.HitWall:
                case GridEvent.StepEnd:
                case GridEvent.RotateEnd:
                    break;
                default:
                    break;
            }

            ExecuteTrigger();
        }
    }
}
