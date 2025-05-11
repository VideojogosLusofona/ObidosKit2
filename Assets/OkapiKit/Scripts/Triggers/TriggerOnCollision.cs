using System;
using System.Collections.Generic;
using UnityEngine;

namespace OkapiKit
{
    [AddComponentMenu("Okapi/Trigger/On Collision")]
    public class TriggerOnCollision : Trigger
    {
        [Flags]
        public enum CollisionType 
        { 
            Collider = 1, 
            Trigger = 2
        }
        public enum CollisionEvent { Enter, Stay, Exit };

        [SerializeField]
        private CollisionType collisionType = CollisionType.Trigger;
        [SerializeField]
        private CollisionEvent eventType;
        [SerializeField]
        private Hypertag[] tags;

        private static GameObject           lastCollider = null;
        private static Stack<GameObject>    lastColliderStack = new();

        bool isTrigger => (collisionType & CollisionType.Trigger) != 0;
        bool isCollider => (collisionType & CollisionType.Collider) != 0;

        public static GameObject GetLastCollider() => lastCollider;
        public static void PushLastCollider(GameObject go) { lastColliderStack.Push(lastCollider); lastCollider = go; }
        public static void PopLastCollider() { lastCollider = lastColliderStack.Pop(); }

        public override string GetTriggerTitle() { return "On Collision"; }

        public override string GetRawDescription(string ident, GameObject refObject)
        {
            var desc = "";
            if (eventType == CollisionEvent.Stay) desc = "While a collision with ";
            else desc = "When a collision with ";

            if (collisionType == (CollisionType.Collider | CollisionType.Trigger)) desc += "any type of collider ";
            else if ((collisionType & CollisionType.Trigger) != 0) desc += "a trigger ";
            else if ((collisionType & CollisionType.Collider) != 0) desc += "a bounding volume ";

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
            switch (eventType)
            {
                case CollisionEvent.Enter: desc += "starts"; break;
                case CollisionEvent.Stay: desc += "happens"; break;
                case CollisionEvent.Exit: desc += "ends"; break;
            }

            return desc;
        }

        protected override void CheckErrors()
        {
            base.CheckErrors();

            if ((tags == null) || (tags.Length == 0))
            {
                _logs.Add(new LogEntry(LogEntry.Type.Error, "No tags defined - OnCollision only detects objects with the given tags!", "OnCollision needs to filter what objects have collided, and it uses the tag list to do so.\nAdd on the Tags list what objects are allowed to collider with this object."));
            }
            else
            {
                int index = 0;
                foreach (var tag  in tags)
                {
                    if (tag == null)
                    {
                        _logs.Add(new LogEntry(LogEntry.Type.Error, $"Empty tags slot {index}!", "An empty slot doesn't do anything, clean up after yourself! :)"));
                    }
                    index++;
                }
            }

            if (GetComponent<Rigidbody2D>() == null)
            {
                _logs.Add(new LogEntry(LogEntry.Type.Error, "OnCollision can only detect collisions if set on an object with a rigid body!", "In Unity, collision events are only reported if there's a rigidbody on the same object as the component that's using those collisions.\nIn this case, that component is this OnCollision trigger."));
            }

            if (collisionType == 0)
            {
                _logs.Add(new LogEntry(LogEntry.Type.Error, "Need to select collider or trigger!", "You need to choose collider or trigger or both to trigger the collisions!"));
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!collision.enabled) return;
            if (!isTrigger) return;
            if (!isTriggerEnabled) return;
            if (eventType != CollisionEvent.Enter) return;
            if (!collision.gameObject.HasHypertags(tags)) return;

            PushLastCollider(collision.gameObject);
            if (EvaluatePreconditions())
            {
                ExecuteTrigger();
            }
            PopLastCollider();
        }


        private void OnCollisionEnter2D(Collision2D collision)
        {
            lastCollider = null;

            if (!collision.enabled) return;
            if (!isCollider) return;
            if (!isTriggerEnabled) return;
            if (eventType != CollisionEvent.Enter) return;
            if (!collision.gameObject.HasHypertags(tags)) return;

            PushLastCollider(collision.otherCollider.gameObject);
            if (EvaluatePreconditions())
            {
                ExecuteTrigger();
            }
            PopLastCollider();
        }
        private void OnTriggerStay2D(Collider2D collision)
        {
            lastCollider = null;

            if (!collision.enabled) return;
            if (!isTrigger) return;
            if (!isTriggerEnabled) return;
            if (eventType != CollisionEvent.Stay) return;
            if (!collision.gameObject.HasHypertags(tags)) return;
            
            PushLastCollider(collision.gameObject);
            if (EvaluatePreconditions())
            {
                ExecuteTrigger();
            }
            PopLastCollider();
        }


        private void OnCollisionStay2D(Collision2D collision)
        {
            lastCollider = null;

            if (!collision.enabled) return;
            if (!isCollider) return;
            if (!isTriggerEnabled) return;
            if (eventType != CollisionEvent.Stay) return;
            if (!collision.gameObject.HasHypertags(tags)) return;

            PushLastCollider(collision.otherCollider.gameObject);
            if (EvaluatePreconditions())
            {
                ExecuteTrigger();
            }
            PopLastCollider();
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            lastCollider = null;

            if (!collision.enabled) return;
            if (!isTrigger) return;
            if (!isTriggerEnabled) return;
            if (eventType != CollisionEvent.Exit) return;
            if (!collision.gameObject.HasHypertags(tags)) return;

            PushLastCollider(collision.gameObject);
            if (EvaluatePreconditions())
            {
                ExecuteTrigger();
            }
            PopLastCollider();
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            lastCollider = null;

            if (!collision.enabled) return;
            if (!isCollider) return;
            if (!isTriggerEnabled) return;
            if (eventType != CollisionEvent.Exit) return;
            if (!collision.gameObject.HasHypertags(tags)) return;

            PushLastCollider(collision.otherCollider.gameObject);
            if (EvaluatePreconditions())
            {
                ExecuteTrigger();
            }
            PopLastCollider();
        }
    }
}
