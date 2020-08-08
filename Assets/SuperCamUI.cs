/* 
*   NatSuite Integrations
*   Copyright (c) 2020 Yusuf Olokoba
*/

namespace NatSuite.Integrations {

    using UnityEngine;
    using UnityEngine.UI;

    public sealed partial class SuperCam {

        enum UIState {
            CameraPreview,
            PhotoCapture,
            VideoPlayback
        }

        UIState state {
            set {
                switch (value) {
                    case UIState.CameraPreview:

                        break;
                    case UIState.PhotoCapture:

                        break;
                    case UIState.VideoPlayback:

                        break;
                }
            }
        }
    }
}