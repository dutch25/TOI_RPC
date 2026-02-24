using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: MelonInfo(typeof(TOI_RPC.ImmortalRPC), "Tale of Immortal Discord RPC", "1.0.0", "Dutch25")]
[assembly: MelonGame("Amazing Seasun Games", "Tale of Immortal")]

namespace TOI_RPC
{
    public class ImmortalRPC : MelonMod
    {
        public static UnitCtrlPlayer ActivePlayer = null;
        private static DiscordRpc.EventHandlers handlers;
        private static DateTime startTime;

        public override void OnApplicationStart()
        {
            MelonLogger.Msg("Initializing Tale of Immortal RPC (Native)...");

            handlers = new DiscordRpc.EventHandlers();
            handlers.readyCallback += ReadyCallback;
            handlers.disconnectedCallback += DisconnectedCallback;
            handlers.errorCallback += ErrorCallback;

            // Reverting to the AppID that was working before
            DiscordRpc.Initialize("1469590602105487390", ref handlers, true, null); 

            startTime = DateTime.UtcNow;

            MelonLogger.Msg("Discord RPC Initialized (Native)!");
        }

        private float lastUpdateTime = 0f;
        private const float UpdateInterval = 10f; // Update every 10 seconds

        public override void OnUpdate()
        {
            DiscordRpc.RunCallbacks();

            if (UnityEngine.Time.time - lastUpdateTime < UpdateInterval) return;
            lastUpdateTime = UnityEngine.Time.time;

            UpdateGamePresence();
        }

        private void UpdateGamePresence()
        {
            try
            {
                if (!ReflectAssembly("Assembly-CSharp", out var asm)) {
                    UpdateRPC("Loading...", "", "toilogo");
                    return;
                }

                var gType = asm.GetType("g");
                if (gType == null) {
                    UpdateRPC("In Menu", "", "toilogo");
                    return;
                }

                // Try to get world via property OR field
                object world = null;
                var worldField = gType.GetField("world", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (worldField != null) world = worldField.GetValue(null);
                
                if (world == null) {
                    var worldProp = gType.GetProperty("world", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    if (worldProp != null) world = worldProp.GetValue(null);
                }

                if (world == null) {
                    UpdateRPC("In Menu", "", "toilogo");
                    return;
                }

                // world.run
                object run = null;
                var runField = world.GetType().GetField("run");
                if (runField != null) run = runField.GetValue(world);
                if (run == null) {
                    var runProp = world.GetType().GetProperty("run");
                    if (runProp != null) run = runProp.GetValue(world);
                }

                if (run == null) {
                    UpdateRPC("In Menu", "", "toilogo");
                    return;
                }

                string name = "Đang tải...";
                string state = "Đang tải...";

                // world.playerUnit
                object playerUnit = null;
                var pUnitProp = world.GetType().GetProperty("playerUnit");
                if (pUnitProp != null) playerUnit = pUnitProp.GetValue(world);

                if (playerUnit != null) {
                    // Try to go deeper into playerUnit.data
                    var pDataProp = playerUnit.GetType().GetProperty("data");
                    if (pDataProp == null) pDataProp = (System.Reflection.PropertyInfo)playerUnit.GetType().GetMember("data").FirstOrDefault(m => m is System.Reflection.PropertyInfo);
                    object pData = null;
                    if (pDataProp != null) pData = pDataProp.GetValue(playerUnit);
                    else {
                        var pDataField = playerUnit.GetType().GetField("data");
                        if (pDataField != null) pData = pDataField.GetValue(playerUnit);
                    }

                    if (pData != null) {
                        // Go into unitData
                        object uData = null;
                        var unitDataProp = pData.GetType().GetProperty("unitData");
                        if (unitDataProp != null) uData = unitDataProp.GetValue(pData);
                        if (uData == null) {
                            var unitDataField = pData.GetType().GetField("unitData");
                            if (unitDataField != null) uData = unitDataField.GetValue(pData);
                        }

                        if (uData != null) {
                            // Extract Name and Grade from propertyData
                            var propDataProp = uData.GetType().GetProperty("propertyData");
                            if (propDataProp != null) {
                                var propData = propDataProp.GetValue(uData);
                                if (propData != null) {
                                    // Name
                                    var nameProp = propData.GetType().GetProperty("name");
                                    if (nameProp != null) {
                                        var val = nameProp.GetValue(propData);
                                        if (val != null) {
                                            if (val is UnhollowerBaseLib.Il2CppStringArray strArray) {
                                                name = string.Join("", strArray);
                                            } else {
                                                name = val.ToString();
                                            }
                                        }
                                    }

                                    // Grade (Realm)
                                    var gradeProp = propData.GetType().GetProperty("gradeID");
                                    if (gradeProp != null) {
                                        var val = gradeProp.GetValue(propData);
                                        if (val != null) {
                                            state = GetRealmName(Convert.ToInt32(val));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // Years and Months from run.roundMonth
                int totalMonths = 0;
                var monthProp = run.GetType().GetProperty("roundMonth");
                if (monthProp != null) totalMonths = (int)monthProp.GetValue(run);
                int years = (totalMonths / 12) + 1;
                int months = (totalMonths % 12) + 1;

                // Final update with Full Info
                UpdateRPC($"{name} (Năm {years}, Tháng {months})", state, "toilogo");
            }
            catch (Exception)
            {
                UpdateRPC("Trong Menu", "", "toilogo");
            }
        }

        private string GetRealmName(int grade)
        {
            int normalizedGrade = grade;
            if (grade > 30) {
                normalizedGrade = grade - 14;
            } else if (grade >= 12) {
                normalizedGrade = grade - 11;
            }

            switch (normalizedGrade)
            {
                case 1: return "Luyện Khí Cảnh (Sơ Kỳ)";
                case 2: return "Luyện Khí Cảnh (Trung Kỳ)";
                case 3: return "Luyện Khí Cảnh (Hậu Kỳ)";
                case 4: return "Trúc Cơ Cảnh (Sơ Kỳ)";
                case 5: return "Trúc Cơ Cảnh (Trung Kỳ)";
                case 6: return "Trúc Cơ Cảnh (Hậu Kỳ)";
                case 7: return "Kết Tinh Cảnh (Sơ Kỳ)";
                case 8: return "Kết Tinh Cảnh (Trung Kỳ)";
                case 9: return "Kết Tinh Cảnh (Hậu Kỳ)";
                case 10: return "Kim Đan Cảnh (Sơ Kỳ)";
                case 11: return "Kim Đan Cảnh (Trung Kỳ)";
                case 12: return "Kim Đan Cảnh (Hậu Kỳ)";
                case 13: return "Cụ Linh Cảnh (Sơ Kỳ)";
                case 14: return "Cụ Linh Cảnh (Trung Kỳ)";
                case 15: return "Cụ Linh Cảnh (Hậu Kỳ)";
                case 16: return "Nguyên Anh Cảnh (Sơ Kỳ)";
                case 17: return "Nguyên Anh Cảnh (Trung Kỳ)";
                case 18: return "Nguyên Anh Cảnh (Hậu Kỳ)";
                case 19: return "Hoá Thần Cảnh (Sơ Kỳ)";
                case 20: return "Hoá Thần Cảnh (Trung Kỳ)";
                case 21: return "Hoá Thần Cảnh (Hậu Kỳ)";
                case 22: return "Ngộ Đạo Cảnh (Sơ Kỳ)";
                case 23: return "Ngộ Đạo Cảnh (Trung Kỳ)";
                case 24: return "Ngộ Đạo Cảnh (Hậu Kỳ)";
                case 25: return "Vũ Hóa Cảnh (Sơ Kỳ)";
                case 26: return "Vũ Hóa Cảnh (Trung Kỳ)";
                case 27: return "Vũ Hóa Cảnh (Hậu Kỳ)";
                case 28: return "Đăng Tiên Cảnh (Sơ Kỳ)";
                case 29: return "Đăng Tiên Cảnh (Trung Kỳ)";
                case 30: return "Đăng Tiên Cảnh (Hậu Kỳ)";
                default: return $"Cảnh giới {grade}";
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(WorldMgr), "IntoWorld")]
        public static class IntoWorldPatch
        {
            public static void Postfix(WorldMgr __instance)
            {
                MelonLogger.Msg("[PATCH] Entered World. Presence should update soon.");
            }
        }

        private bool ReflectAssembly(string name, out System.Reflection.Assembly asm)
        {
            asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == name);
            return asm != null;
        }

        public override void OnApplicationQuit()
        {
            DiscordRpc.Shutdown();
        }

        private void UpdateRPC(string details, string state, string largeImg)
        {
            var presence = new DiscordRpc.RichPresence()
            {
                details = details,
                state = state,
                startTimestamp = (long)(startTime - new DateTime(1970, 1, 1)).TotalSeconds,
                largeImageKey = largeImg,
                largeImageText = "Tale of Immortal"
            };

            DiscordRpc.UpdatePresence(ref presence);
            MelonLogger.Msg($"RPC Pending Update: {details} - {state}");
        }

        private void ReadyCallback(ref DiscordRpc.DiscordUser user)
        {
            MelonLogger.Msg($"Discord RPC Ready! User: {user.username}");
        }

        private void DisconnectedCallback(int errorCode, string message)
        {
            MelonLogger.Error($"Discord RPC Disconnected: {errorCode} - {message}");
        }

        private void ErrorCallback(int errorCode, string message)
        {
            MelonLogger.Error($"Discord RPC Error: {errorCode} - {message}");
        }
    }
}
