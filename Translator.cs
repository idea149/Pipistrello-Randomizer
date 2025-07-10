using Il2CppPipistrello;
using MelonLoader;

namespace Randomizer
{
    public class ObjectId
    {
        public string mapId = "";
        public string roomId = "";
        public string objectId = "";

        public ObjectId(Game.GlobalObjectId gid)
        {
            mapId = gid.mapId;
            roomId = gid.roomId;
            objectId = gid.objectId;
        }

        public static bool operator ==(ObjectId left, ObjectId right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (ReferenceEquals(left, null)) return false;
            if (ReferenceEquals(right, null)) return false;
            return left.mapId == right.mapId && left.roomId == right.roomId && left.objectId == right.objectId;
        }

        public static bool operator !=(ObjectId left, ObjectId right) => !(left == right);

        public override bool Equals(object obj)
        {
            return Equals(obj as ObjectId);
        }
        public virtual bool Equals(ObjectId obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (object.ReferenceEquals(this, obj))
            {
                return true;
            }
            return this.mapId == obj.mapId &&
                this.roomId == obj.roomId &&
                this.objectId == obj.objectId;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(mapId, roomId, objectId);
        }

        public override string ToString()
        {
            return mapId + "/" + roomId + "/" + objectId;
        }
    }

    public class Translator
    {
        public static readonly string randomizerEnabledFlag = "g:randomizer:enabled";
        public static readonly string randomizerSeedAFlag = "g:randomizer:seedA";
        public static readonly string randomizerSeedBFlag = "g:randomizer:seedB";

        // Object translation map
        public static Dictionary<ObjectId, Mapvania.Object> objMap = new Dictionary<ObjectId, Mapvania.Object>();

        // vanilla maps to get object ids from
        public static readonly HashSet<string> vanillaMapList = new HashSet<string> {
            "cinema",
            "city_interiors",
            "city_underground",
            "city",
            "dungeon1",
            "dungeon2",
            "dungeon3",
            "dungeon4",
            "dungeon5",
            "escapehouse",
            "factory",
            "museum",
            "policedep",
            "safehouse",
            "skyscraper",
            "subway",
            "tunnels"
        };

        private static List<Mapvania.Object> GenObjectPool()
        {
            List<Mapvania.Object> res = new List<Mapvania.Object>();

            Mapvania.Project proj = new Mapvania.Project();
            Mapvania.ReloadMaps(proj);
            foreach (Mapvania.Map map in proj.maps)
            {
                if (!vanillaMapList.Contains(map.id))
                {
                    continue;
                }
                foreach (Mapvania.Room room in map.rooms)
                {
                    foreach (Mapvania.Object obj in room.objects)
                    {
                        if (obj.objectDefName == "equip" || obj.objectDefName == "petalContainer" || obj.objectDefName == "bpContainer")
                        {
                            res.Add(obj);
                        }
                    }
                }
            }

            return res;
        }

        public static void Randomize(int seed)
        {
            MelonLogger.Msg($"Using seed {seed}");
            objMap.Clear();
            MelonLogger.Msg("Randomizing");
            Random rand = new Random(seed);
            List<Mapvania.Object> equipsSrc = GenObjectPool();
            List<Mapvania.Object> equipsShuffled = equipsSrc
                .OrderBy(x => rand.Next())
                .ToList();

            for (int i = 0; i < equipsSrc.Count; i++)
            {
                objMap.Add(new ObjectId(equipsSrc[i].globalObjectId), equipsShuffled[i]);
                MelonLogger.Msg(equipsSrc[i].globalObjectId.AsString + " <=> " + equipsShuffled[i].globalObjectId.AsString);
            }
        }

        public static void TranslateObj(ref Mapvania.Object obj)
        {
            if (obj == null)
            {
                return;
            }

            ObjectId key = new ObjectId(obj.globalObjectId);
            if (objMap.ContainsKey(key))
            {
                // Copy relevant properties
                Il2CppUtil.JsonValue originalProperties = obj.properties;
                Il2CppUtil.JsonValue properties = new Il2CppUtil.JsonValue();
                properties.kind = Il2CppUtil.JsonValue.Kind.Object;
                properties.objectFields = new Il2CppSystem.Collections.Generic.List<Il2CppUtil.JsonValue.Field>();

                // properties to keep from the original obj
                List<string> propertiesToKeep = new List<string> { "winged", "presenceFlag" };
                foreach (var field in originalProperties.Fields)
                {
                    if (propertiesToKeep.Contains(field.name))
                    {
                        properties.SetField(field.name, field.value);
                    }
                }

                // Copy all other properties from the target obj
                foreach (var field in objMap[key].properties.Fields)
                {
                    if (!propertiesToKeep.Contains(field.name))
                    {
                        properties.SetField(field.name, field.value);
                    }
                }

                // Set 
                obj.objectDefName = objMap[key].objectDefName;
                obj.objectDefId = objMap[key].objectDefId;
                obj.properties = properties;
            }
        }
    }
}
