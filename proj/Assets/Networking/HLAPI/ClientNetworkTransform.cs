using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TransformSnapshot = VirtualVoid.Net.NetworkTransform.TransformSnapshot;
using TransformUpdateFlags = VirtualVoid.Net.NetworkTransform.TransformUpdateFlags;

namespace VirtualVoid.Net
{
    public class ClientNetworkTransform : MonoBehaviour
    {
        [Tooltip("When should this try to sync?")]
        public SyncUpdateLoop updateLoop;

        private bool syncsWithFixedUpdate => updateLoop == SyncUpdateLoop.FixedUpdate;

        [Min(0f)]
        [Tooltip("If not syncing with FixedUpdate, try to sync this many times per second. 0 for every frame (not recommended).")]
        public int syncsPerSecond = 0;
        private int lastSyncsPerSecond = 0;
        private float syncTime = 0.1f;
        public NetworkTransformSettings settings;

        internal Vector3 lastPosition = Vector3.zero;
        internal Quaternion lastRotation = Quaternion.identity;
        internal Vector3 lastScale = Vector3.one;
        private float lastSyncTime = 0;

        public Client ownerClient;

        private float syncDelay => syncsWithFixedUpdate ? Time.fixedDeltaTime : syncTime;
        private float currentInterpolation
        {
            get
            {
                float difference = target.time - current.time;

                float elapsed = Time.time - target.time;
                return difference > 0 ? elapsed / difference : 0;
                // Thanks mirror for this useful bit of code
            }
        }

        private readonly TransformSnapshot current = new TransformSnapshot();
        internal readonly TransformSnapshot target = new TransformSnapshot();

        private const float SNAP_THRESHOLD_MULTIPLIER = 10;

        private void Start()
        {
            if (syncsPerSecond == 0) syncTime = 0; // Avoid division by zero
            else syncTime = 1f / syncsPerSecond;

            lastPosition = settings.position.useGlobal ? transform.position : transform.localPosition;
            lastRotation = settings.rotation.useGlobal ? transform.rotation : transform.localRotation;
            lastScale = transform.localScale;

            current.Update(lastPosition, lastRotation, lastScale, Time.time - syncDelay);
            target.Update(lastPosition, lastRotation, lastScale, Time.time);
        }

        private void Update()
        {
            if (!SteamManager.AllClientsLoadedInScene()) return;

            if (syncsPerSecond != lastSyncsPerSecond)
            {
                lastSyncsPerSecond = syncsPerSecond;
                if (syncsPerSecond == 0) syncTime = 0; // Avoid division by zero
                else syncTime = 1f / syncsPerSecond;
            }

            if (updateLoop != SyncUpdateLoop.Update) return;

            TransformStuffForFrameUpdates();
        }

        private void LateUpdate()
        {
            if (!SteamManager.AllClientsLoadedInScene()) return;

            if (updateLoop != SyncUpdateLoop.LateUpdate) return;

            TransformStuffForFrameUpdates();
        }

        // Not a super useful name. Function just so in LateUpdate and Update I dont write the same crap twice
        private void TransformStuffForFrameUpdates()
        {
            if (ownerClient.IsLocalPlayer && Time.time - lastSyncTime > syncTime)
            {
                lastSyncTime = Time.time;
                CheckTransform();
            }

            UpdateTransform();
        }

        private void FixedUpdate()
        {
            if (!SteamManager.AllClientsLoadedInScene()) return;

            if (updateLoop != SyncUpdateLoop.FixedUpdate) return;

            CheckTransform();
            UpdateTransform();
        }

        private void CheckTransform()
        {
            if (!ownerClient.IsLocalPlayer) return; // && !clientAuthority) return;

            TransformUpdateFlags flags = TransformUpdateFlags.NONE;

            if (settings.position.sync && HasMoved())
                flags |= TransformUpdateFlags.POSITION;

            if (settings.rotation.sync && HasRotated())
                flags |= TransformUpdateFlags.ROTATION;

            if (settings.scale.sync && HasScaled())
                flags |= TransformUpdateFlags.SCALE;

            //Debug.Log(flags.ToString()); Works correctly

            if (flags == TransformUpdateFlags.NONE) return;

            InternalClientSend.SendNetworkTransform(this, flags);
            //Message message = Message.CreateInternal(P2PSend.Reliable, (ushort)InternalServerMessageIDs.NETWORK_TRANSFORM);
            //
            //message.Add((byte)flags);
            //if (flags.HasFlag(TransformUpdateFlags.POSITION)) message.Add(lastPosition);
            //if (flags.HasFlag(TransformUpdateFlags.ROTATION)) message.Add(lastRotation);
            //if (flags.HasFlag(TransformUpdateFlags.SCALE)) message.Add(lastScale);
            //
            //SteamManager.SendMessageToAllClients(message);
        }

        internal TransformUpdateFlags GetFlags()
        {
            TransformUpdateFlags flags = TransformUpdateFlags.NONE;

            if (settings.position.sync && HasMoved())
                flags |= TransformUpdateFlags.POSITION;

            if (settings.rotation.sync && HasRotated())
                flags |= TransformUpdateFlags.ROTATION;

            if (settings.scale.sync && HasScaled())
                flags |= TransformUpdateFlags.SCALE;

            return flags;
        }

        private void UpdateTransform()
        {
            if (ownerClient.IsLocalPlayer) return;

            // Interpolate / Snap
            if (settings.position.sync)
            {
                if (settings.position.interpolate)
                {
                    if (settings.position.useGlobal)
                        transform.position = Vector3.Lerp(current.position, target.position, currentInterpolation);
                    else
                        transform.localPosition = Vector3.Lerp(current.position, target.position, currentInterpolation);
                }
                else
                {
                    if (settings.position.useGlobal)
                        transform.position = target.position;
                    else
                        transform.localPosition = target.position;
                }
            }

            if (settings.rotation.sync)
            {
                if (settings.rotation.interpolate)
                {
                    if (settings.rotation.useGlobal)
                        transform.rotation = Quaternion.Slerp(current.rotation, target.rotation, currentInterpolation);
                    else
                        transform.localRotation = Quaternion.Slerp(current.rotation, target.rotation, currentInterpolation);
                }
                else
                {
                    if (settings.rotation.useGlobal)
                        transform.rotation = target.rotation;
                    else
                        transform.localRotation = target.rotation;
                }
            }

            if (settings.scale.sync)
            {
                if (settings.scale.interpolate)
                {
                    transform.localScale = Vector3.Lerp(current.scale, target.scale, currentInterpolation);
                }
                else
                {
                    transform.localScale = target.scale;
                }
            }
        }

        private bool HasMoved()
        {
            Vector3 currentPos = settings.position.useGlobal ? transform.position : transform.localPosition;
            bool changed = Vector3.Distance(lastPosition, currentPos) > settings.position.sensitivity;
            if (changed)
                lastPosition = currentPos;

            return changed;
        }

        private bool HasRotated()
        {
            Quaternion currentRot = settings.rotation.useGlobal ? transform.rotation : transform.localRotation;
            bool changed = Quaternion.Angle(lastRotation, currentRot) > settings.rotation.sensitivity;
            if (changed)
                lastRotation = currentRot;

            return changed;
        }

        private bool HasScaled()
        {
            Vector3 currentScale = transform.localScale;
            bool changed = Vector3.Distance(lastScale, currentScale) > settings.scale.sensitivity;
            if (changed)
                lastScale = currentScale;

            return changed;
        }

        private bool ShouldSnap()
        {
            float currentTime = current == null ? Time.time - syncDelay : current.time;
            float targetTime = target == null ? Time.time : target.time;
            float difference = targetTime - currentTime;
            float timeSinceGoalReceived = Time.time - targetTime;
            return timeSinceGoalReceived > difference * SNAP_THRESHOLD_MULTIPLIER;

            //if (current == null) return true;
            //
            //if (syncWithFixedUpdate)
            //    return target.time - current.time > Time.fixedDeltaTime * SNAP_THRESHOLD_MULTIPLIER;
            //else
            //    return target.time - current.time > syncTime * SNAP_THRESHOLD_MULTIPLIER;
        }

        internal void OnNewTransformReceived(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            if (ShouldSnap()) current.Update(target.position, target.rotation, target.scale, target.time);
            else current.Update(settings.position.useGlobal ? transform.position : transform.localPosition,
                settings.rotation.useGlobal ? transform.rotation : transform.localRotation, transform.localScale, Time.time - syncDelay);

            target.Update(position, rotation, scale, Time.time);
        }

        [ContextMenu("Quantization test")]
        public void QuantizationTest()
        {
            if (settings == null || !settings.position.quantize) return;

            Vector3 pos = transform.position;
            Vector3 min = settings.position.quantizationRangeMin;
            Vector3 max = settings.position.quantizationRangeMax;
            ushort x = Compression.Vector.Quantize_16bit(pos.x, min.x, max.x, 16);
            ushort y = Compression.Vector.Quantize_16bit(pos.y, min.y, max.y, 16);
            ushort z = Compression.Vector.Quantize_16bit(pos.z, min.z, max.z, 16);

            float d_x = Compression.Vector.Dequantize(x, min.x, max.x, 16);
            float d_y = Compression.Vector.Dequantize(y, min.y, max.y, 16);
            float d_z = Compression.Vector.Dequantize(z, min.z, max.z, 16);

            Debug.Log($"Current pos - {pos}");
            Debug.Log($"Quantized pos - ({x}, {y}, {z})");
            Debug.Log($"Dequantized pos - ({d_x}, {d_y}, {d_z})");
            //Debug.Log

        }
    }
}
