using Zealot.Entities;

namespace Zealot.Spawners
{
    public abstract class RealmController : ServerEntityWithEvent
    {
        void Awake()
        {
            gameObject.tag = "EditorOnly";
        }

        public override string[] Triggers
        {
            get
            {
                return new string[] { "CompleteRealm" };
            }
        }

        public override ServerEntityJson GetJson()
        {
            RealmControllerJson jsonclass = new RealmControllerJson();
            GetJson(jsonclass);
            return jsonclass;
        }

        public void GetJson(RealmControllerJson jsonclass)
        {
            jsonclass.forward = transform.forward;
            jsonclass.forward.y = 0;
            base.GetJson(jsonclass);
        }
    }
}
