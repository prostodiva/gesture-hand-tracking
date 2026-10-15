> JointPinchSelect = uses hand joints (XR Hands subsystem) to detect a pinch and drives XRI's Select
> The Scripts sits on a Left Hand GameObject. We need the script to send a Select signal to Near-Far Interactor (XRI)

# The flow
headset / simulator uses XR Hands subsystem (joints)
JointPinchSelect, every frame:
  - find the running hand subsystem
  - read thumb tip + index tip of this hand
  - distance < 2 cm -> pinching
    distance > 3.5 cm -> releasing
  - interactor.selectInput.QueueManualState(pinching, value)
  - Near-Far Interactor sees Select = pressed
  - XR Grab Interactable on the cube -> attached to the hand

# Implementing steps
  - setup the fields & variables
  - Update() for each frame:
      - check if the interactor field is set
      - TakeOver() - take control of Select 
          - if control is taken - return
          - save the original mode
          - change mode to manual
          - set the flag
      - TryGetTipDistance() - meassure the fingers:
          - take the fingers position
          - calculate 
          - update
      - check the edge case:
          - let it go
      - check the distance, calculate
      - send the result: interactor.selectInput.QueueManualState(pinching, value);
      - when the component is disabled -> HandBack() - restore the original mode
      - 
      -
