using Kopio.JsonContracts;
using System.Collections.Generic;
using Zealot.Common;

namespace Zealot.Repository
{
    public struct PartyLocationData
    {
        public int locationId;
        public string localizedName;
        public LocationType locationType;

        public PartyLocationData(int id, string name, LocationType type)
        {
            locationId = id;
            localizedName = name;
            locationType = type;
        }
    }

    public static class PartyRepo
    {
        public static Dictionary<int, PartyLocationData> locations;
        public static Dictionary<LocationType, List<PartyLocationData>> locationsByType;

        static PartyRepo()
        {
            locations = new Dictionary<int, PartyLocationData>();
            locationsByType = new Dictionary<LocationType, List<PartyLocationData>>();
        }

        public static void Init(GameDBRepo gameData)
        {
            locations.Clear();
            locationsByType.Clear();

            foreach (var entry in gameData.PartyLocation)
            {
                if (entry.Value.locationtype == LocationType.Nearby)
                    continue;

                string[] realmIds = entry.Value.realmid.Split(';');
                for (int i = 0; i < realmIds.Length; i++)
                {
                    int realmId;
                    if (int.TryParse(realmIds[i], out realmId))
                    {
                        RealmJson realmJson = RealmRepo.GetInfoById(realmId);
                        if (realmJson != null)
                        {
                            PartyLocationData locationData = new PartyLocationData(realmId, realmJson.localizedname, entry.Value.locationtype);
                            locations.Add(realmId, locationData);

                            if (locationsByType.ContainsKey(entry.Value.locationtype))
                                locationsByType[entry.Value.locationtype].Add(locationData);
                            else
                                locationsByType.Add(entry.Value.locationtype, new List<PartyLocationData>() { locationData });
                        }
                    }                 
                }
            }
        }

        public static string GetLocationName(int locId)
        {
            if (locId == 0)
                return GUILocalizationRepo.GetLocalizedString("pty_nearby");
            else
            {
                PartyLocationData data;
                if (locations.TryGetValue(locId, out data))
                    return data.localizedName;
                return "";
            }
        }

        public static List<PartyLocationData> GetLocationsByType(LocationType type)
        {
            List<PartyLocationData> list;
            locationsByType.TryGetValue(type, out list);
            return list;
        }

        public static string FormatLevelRange(int min, int max)
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("min", min.ToString());
            param.Add("max", max.ToString());
            return GUILocalizationRepo.GetLocalizedString("com_levelrange", param);
        }
    }
}