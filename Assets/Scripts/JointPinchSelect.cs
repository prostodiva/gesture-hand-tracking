using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class JointPinchSelect : MonoBehaviour
{
    //reads input: selectInput, activateInput; holds the ref to Near-Far Interactor
    // so the script know which interactor to control
    [SerializeField] private XRBaseInputInteractor interactor;

    [SerializeField] private Handedness handedness = Handedness.Right; //which hand
    [SerializeField] private float pinchDistance = 0.02f; //start at 2 cm
    [SerializeField] private float releaseDistance = 0.035f; //end at 3.5 cm

    //a reusable list, filled by GetSubsystems, so a new list isn’t created each time
    private static readonly List<XRHandSubsystem> subsystems = new List<XRHandSubsystem>();

    private XRHandSubsystem hands;
    private XRInputButtonReader.InputSourceMode originalMode;
    private bool inControl; //starts as false
    private bool pinching;

    // no Awake, Start - nothing runs until the first frame

    // when the component is disabled -  Gives Select back to its original input source
    // I untick the JointPinchSelect checkbox || GameObject checkbox || the object is destroyed and etc.
    private void OnDisable() => HandBack();

    //Update is called by Unity every frame. How many frames depends on the device
    private void Update()
    {
        // do we have an interactor set? (Near-Far Interactor)
        // I dragged the Near-Far Interactor to the field
        if (interactor == null) return;

        //before take over - select comes from an input action
        //take control of Select
        //meaning - Stop the interactor from reading Select from the input action, and make it read Select from this script instead
        TakeOver();

        if (!TryGetTipDistance(out float distance))
        {
            // Hand lost: let go rather than keep holding with stale data.
            pinching = false;
            interactor.selectInput.QueueManualState(false, 0f);
            return;
        }

        // Two thresholds: start below one, stop only above the other.
        pinching = pinching ? distance < releaseDistance : distance < pinchDistance;

        //send the result
        float value = Mathf.InverseLerp(releaseDistance, pinchDistance, distance);
        //QueueManualState is a method of XRInputButtonReader(the type of selectInput)
        //It works out whether Select was just pressed or just released by comparing with the current state,
        //and stores the new state to take effect on the next frame.
        //When the interactor next reads its Select input, XRI applies the stored state, and the interactor starts or ends the grab.
        interactor.selectInput.QueueManualState(pinching, value);

    }

    //switch the control to manual
    private void TakeOver()
    {
        if (inControl) return; //if the control is already taken - return; starts at false

        originalMode = interactor.selectInput.inputSourceMode; //save the original for restoring later
        interactor.selectInput.inputSourceMode = XRInputButtonReader.InputSourceMode.ManualValue; //change to manual
        inControl = true;
        //from now on, the interactor ignores the Select input action. Its Select is only what this script sends
    }

    // Gives Select back to its original input source
    private void HandBack()
    {
        if (!inControl || interactor == null) return;

        interactor.selectInput.QueueManualState(false, 0f); //release
        interactor.selectInput.inputSourceMode = originalMode; //restore the mode
        //reset the flags
        inControl = false;
        pinching = false;
    }

    //measure the fingers
    private bool TryGetTipDistance(out float distance)
    {
        distance = 0f;

        if (hands == null || !hands.running)
        {
            hands = FindRunningSubsystem(); //the simulator on OpenXR's
            if (hands == null) return false;
        }

        XRHand hand = handedness == Handedness.Left ? hands.leftHand : hands.rightHand; //take the hand's data
        if (!hand.isTracked) return false;

        //take the fingers position
        if (!hand.GetJoint(XRHandJointID.ThumbTip).TryGetPose(out Pose thumb)) return false;
        if (!hand.GetJoint(XRHandJointID.IndexTip).TryGetPose(out Pose index)) return false;

        distance = Vector3.Distance(thumb.position, index.position);
        return true; //back to update
    }

    //checking whick one is turned on: the simulator or OpenXR's
    private static XRHandSubsystem FindRunningSubsystem()
    {
        SubsystemManager.GetSubsystems(subsystems);

        foreach (XRHandSubsystem subsystem in subsystems)
            if (subsystem.running) return subsystem;

        return null;
    }
}
