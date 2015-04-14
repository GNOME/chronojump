NOT INERTIAL
------------

(1) CAPTURE

for each repetition (curve)
- C#  src/gui/encoder/runEncoderCaptureCsharp() sends compressed displacement.
  First checking change of direction, search the limits. Limits are supposed to be this:
	1.- Curve starting frame:
		a) If it's first curve from the beginning of the signal
		b) If not, from the end of previous curve
	2.- Curve ending frame:
		central point at displacements == 0
  	TODO: really check if everything is working as supposed. And if this is the best method.
- capture.R reads line by line
- uncompres it
- maybe displacement changes by encoderConfiguration
- (first smoothing) reduceCurveBySpeed cuts the curve from the last and first speed=0 at each side #TODO maybe use just position maxs and mins (extrema)
- kinematicsF

(2) LOAD SIGNAL OR END OF CAPTURE (CUR)

- graph.R doProcess singleFile == TRUE
- reads uncompressed displacement
- maybe displacement changes by encoderConfiguration
- findCurves !!!! cut signal by extrema criterion
- reduceCurveBySpeed: cut curves by speed == 0
... process is same as in singleFile == FALSE

(3) PROCESS ONE SAVED CURVE

- graph.R doProcess singleFile == FALSE
- curves are in separated files already cutted previously (this makes this data is the same than (2)

CONCLUSIONS

(2) and (3) are equal
(1) sometimes is different. Check if:
	(1) data sent by C# on capture 
	== 
	(3) data stored for each curve

