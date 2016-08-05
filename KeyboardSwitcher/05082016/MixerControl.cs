using System;
using System.Runtime.InteropServices;

namespace PostSwitcher
{
    internal class MixerControl
    {
        private  IAudioEndpointVolume _volume;

        public MixerControl()
        {
            // Получаем аудио устройство
            IMMDeviceEnumerator deviceEnumerator = (IMMDeviceEnumerator)(new MMDeviceEnumerator());
            IMMDevice speakers;
            deviceEnumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, out speakers);

            // Активируем менеджер сессий, нам нужен главный регулятор громкости
            Guid IID_IAudioEndpointVolume = typeof(IAudioEndpointVolume).GUID;
            object o;
            speakers.Activate(ref IID_IAudioEndpointVolume, 0, IntPtr.Zero, out o);
            _volume = (IAudioEndpointVolume)o;

            Marshal.ReleaseComObject(speakers);
            Marshal.ReleaseComObject(deviceEnumerator);
        }

         ~MixerControl()
         {
             Marshal.ReleaseComObject(_volume);
         }


        public float MasterVolume 
        {
            get
            {
                float level;
                _volume.GetMasterVolumeLevelScalar(out level);
                return level*100f;
            }
            set
            {
                float level = value;
                if (level > 100f) level = 100f;
                if (level < 0f) level = 0f;
                _volume.SetMasterVolumeLevelScalar(level/100f, Guid.Empty);
            }
        }

        public bool Mute
        {
            get
            {
                bool mute;
                _volume.GetMute(out mute);
                return mute;
            }
            set { _volume.SetMute(value, Guid.Empty); }
        }

        #region Core Audio Declarations

        //https://github.com/morphx666/CoreAudio/tree/master/CoreAudio
        //http://stackoverflow.com/questions/14306048/controling-volume-mixer
        //http://ru.stackoverflow.com/questions/453851/%D0%9A%D0%B0%D0%BA-%D0%BF%D1%80%D0%B0%D0%B2%D0%B8%D0%BB%D1%8C%D0%BD%D0%BE-%D1%80%D0%B5%D0%B3%D1%83%D0%BB%D0%B8%D1%80%D0%BE%D0%B2%D0%B0%D1%82%D1%8C-%D1%83%D1%80%D0%BE%D0%B2%D0%B5%D0%BD%D1%8C-%D0%B3%D1%80%D0%BE%D0%BC%D0%BA%D0%BE%D1%81%D1%82%D0%B8



        private enum EDataFlow
        {
            eRender = 0,
            eCapture = 1,
            eAll = 2,
            EDataFlow_enum_count = 3
        }

        private enum ERole
        {
            eConsole = 0,
            eMultimedia = 1,
            eCommunications = 2,
            ERole_enum_count = 3
        }

        [Guid("D666063F-1587-4E43-81F1-B948E807363F"),
         InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IMMDevice
        {
            [PreserveSig]
            int Activate(ref Guid iid, int dwClsCtx, IntPtr pActivationParams,
                [Out, MarshalAs(UnmanagedType.IUnknown)] out object ppInterface);

        }

        [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"),
         InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IMMDeviceEnumerator
        {
            //[PreserveSig]
            //int EnumAudioEndpoints(EDataFlow dataFlow, DEVICE_STATE StateMask, out IMMDeviceCollection device);
            int NotImpl1();

            [PreserveSig]
            int GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role, out IMMDevice ppEndpoint);
        }

        [ComImport, Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
        private class MMDeviceEnumerator
        {
        }


        [Guid("5CDF2C82-841E-4546-9722-0CF74078229A"),
         InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IAudioEndpointVolume
        {
            [PreserveSig]
            int RegisterControlChangeNotify(IAudioEndpointVolumeCallback pNotify);

            [PreserveSig]
            int UnregisterControlChangeNotify(IAudioEndpointVolumeCallback pNotify);

            [PreserveSig]
            int GetChannelCount(out int pnChannelCount);

            [PreserveSig]
            int SetMasterVolumeLevel(float fLevelDB, Guid pguidEventContext);

            [PreserveSig]
            int SetMasterVolumeLevelScalar(float fLevel, Guid pguidEventContext);

            [PreserveSig]
            int GetMasterVolumeLevel(out float pfLevelDB);

            [PreserveSig]
            int GetMasterVolumeLevelScalar(out float pfLevel);

            [PreserveSig]
            int SetChannelVolumeLevel(uint nChannel, float fLevelDB, Guid pguidEventContext);

            [PreserveSig]
            int SetChannelVolumeLevelScalar(uint nChannel, float fLevel, Guid pguidEventContext);

            [PreserveSig]
            int GetChannelVolumeLevel(uint nChannel, out float pfLevelDB);

            [PreserveSig]
            int GetChannelVolumeLevelScalar(uint nChannel, out float pfLevel);

            [PreserveSig]
            int SetMute([MarshalAs(UnmanagedType.Bool)] bool bMute, Guid pguidEventContext);

            [PreserveSig]
            int GetMute(out bool pbMute);

            [PreserveSig]
            int GetVolumeStepInfo(out uint pnStep, out uint pnStepCount);

            [PreserveSig]
            int VolumeStepUp(Guid pguidEventContext);

            [PreserveSig]
            int VolumeStepDown(Guid pguidEventContext);

            [PreserveSig]
            int QueryHardwareSupport(out uint pdwHardwareSupportMask);

            [PreserveSig]
            int GetVolumeRange(out float pflVolumeMindB, out float pflVolumeMaxdB, out float pflVolumeIncrementdB);
        }

        [Guid("657804FA-D6AD-4496-8A60-352752AF4F89"),
         InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IAudioEndpointVolumeCallback
        {
            [PreserveSig]
            int OnNotify(IntPtr pNotifyData);
        }

        #endregion
    }
}

