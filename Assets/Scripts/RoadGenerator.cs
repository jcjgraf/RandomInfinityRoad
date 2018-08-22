using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadGenerator : MonoBehaviour {

	public Transform car;

	public RoadSegment[] roadSegments;

	private Vector3 segmentStartPosition;
	private Vector3 segmentStartRotation;

	private Queue<Transform> roadSegmentsQueue;

	private GameObject lastRoadSegment;

	private int numberOfSegments;

	private int lastIndex;

	private List<Vector3> startPositionList;
	private List<Vector3> startRotationList;

	// todo temp
	private List<int> numberList;

	// Needed since it might trigger several collisions in the same frame
	private bool hasTriggered = false;

	void Awake () {

		TriggerManager.colliderTriggered += trigger;

		roadSegmentsQueue = new Queue<Transform>();
		segmentStartPosition = new Vector3(0, 0, 0);
		segmentStartRotation = new Vector3(0, 0, 0);

		numberOfSegments = 5;
		lastIndex = 0;

		//TODO temp
		numberList = new List<int>() { 0, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0};



		generateRoad(0);
	}
	
	void Update () {
		/*
			Get the name resp. its number of the first segment in the roadSegmentsQueme and compare it the with segment the car is on currently. If it has passed the first segment, destroy it and generate a new one.
		*/

		int firstSegment;
		int.TryParse(roadSegmentsQueue.ToArray()[0].gameObject.GetComponentInChildren<Transform>().GetChild(0).name, out firstSegment);

		if (getCurrentSegment() > firstSegment + 1) { // + 1 in order to always leave a segment

			Transform toRemove = roadSegmentsQueue.Dequeue();
			Destroy(toRemove.gameObject);


		}

		generateRoad();

		hasTriggered = false;

		// Debug
//		GameObject mySphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
//		mySphere.transform.position = segmentStartPosition;

	}

	int getCurrentSegment() {
		/*	
			Use a raycast in order at the center of the car in order to find out the object which the car is currently on.	
		*/

		RaycastHit hit;

		Vector3 rayUnitVector = new Vector3(0, -1, 0);

		if (Physics.Raycast(car.position, rayUnitVector, out hit, Mathf.Infinity)) {

			Debug.DrawLine(car.position, hit.point, Color.red);

			int colliderName;
			int.TryParse(hit.collider.name, out colliderName);

			return colliderName;
		}

		return 0;
	}

	void generateRoad() {

		if (roadSegmentsQueue.ToArray().Length == numberOfSegments) {
			return;
		}

		addSegment();
		generateRoad();
	}

	void generateRoad(int segmentInt) {

		if (roadSegmentsQueue.Count == numberOfSegments) {
			return;
		}

		addSegment(segmentInt);
		generateRoad(segmentInt);
	}

	void addSegment() {

		addSegment(Random.Range(0, roadSegments.Length -1 ));
	}

	void addSegment(int segmentInt) {

		RoadSegment newRoadSegment;

		if (segmentInt > roadSegments.Length) {
			// Ugly way to generate more straight segments
			newRoadSegment = roadSegments[0];

		} else {
			newRoadSegment = roadSegments[segmentInt];
		}

		// TODO Temp
		newRoadSegment = roadSegments[numberList[0]];
		numberList.RemoveAt(0);

		GameObject newSegment = Instantiate<GameObject>(newRoadSegment.prefab);

		// Start used for detecting collisions
		// Setup Rigidbody
		Rigidbody newSegementRigidbody =  newSegment.AddComponent<Rigidbody>();
		newSegementRigidbody.isKinematic = true;

		// Add the class with the triggerEvent to the newly created gameObject
		newSegment.AddComponent(typeof(TriggerManager));

		// Add collider and set right properties
		MeshCollider newSegmentCollider = newSegment.gameObject.GetComponentInChildren<Transform>().GetChild(0).GetComponent<MeshCollider>();

		newSegmentCollider.convex = true;
		newSegmentCollider.isTrigger = true;

		// End used for detecting collisions

		// Transform new gameObject
		newSegment.transform.position = segmentStartPosition + newRoadSegment.standardPosition;
		newSegment.transform.eulerAngles = segmentStartRotation + newRoadSegment.standardRotation;
		newSegment.transform.localScale = newRoadSegment.standardScale;

		// Set name of the road Segment
		newSegment.gameObject.GetComponentInChildren<Transform>().GetChild(0).name = lastIndex.ToString();

		lastIndex++;

		// Update position and rotation
		// Deg to Rad
		float angle = -1 * (segmentStartRotation.y * Mathf.PI / 180);  // The -1 is needed since the rotation matrix works in the other way around as the unity rotation system
		// Apply a rotation vectory
		// Multiply by the roation matrix
		Vector3 deltaMove = new Vector3(newRoadSegment.deltaEndPosition.x * Mathf.Cos(angle) - newRoadSegment.deltaEndPosition.z * Mathf.Sin(angle), 0, newRoadSegment.deltaEndPosition.x * Mathf.Sin(angle) + newRoadSegment.deltaEndPosition.z * Mathf.Cos(angle));
	
		segmentStartPosition = segmentStartPosition + deltaMove;
		segmentStartRotation = segmentStartRotation + newRoadSegment.deltaEndRotation;

		roadSegmentsQueue.Enqueue(newSegment.transform);

		// remove trigger detection thingis of the second last road segment since there is already a new one in place on the new last one
		try {
			removeTriggerDetection(lastRoadSegment);
		}
		catch {}

		lastRoadSegment = newSegment;
	}

	void removeTriggerDetection(GameObject segment) {

		// Undo changes
		segment.gameObject.GetComponentInChildren<Transform>().GetChild(0).GetComponent<MeshCollider>().isTrigger = false;
		segment.gameObject.GetComponentInChildren<Transform>().GetChild(0).GetComponent<MeshCollider>().convex = false;
		Destroy(segment.GetComponent<TriggerManager>());

	}

	void trigger(Collider collider) {

		int colliderNameInt = 0;
		int segmentNameInt = 0;

		bool parseI = int.TryParse (collider.gameObject.name, out colliderNameInt);
		bool parseII = int.TryParse (lastRoadSegment.GetComponentInChildren<Transform> ().GetChild (0).name, out segmentNameInt);

		// If both parse succeedes
		if (parseI && parseII && hasTriggered == false) {

			if (colliderNameInt + 1 == segmentNameInt || colliderNameInt == segmentNameInt) {
				// Debug.Log("Harmless collision");
			} else {
//				Debug.Log ("Fatal collision");
				Debug.Log ("Collided with " + collider.gameObject.name + " - " + lastRoadSegment.GetComponentInChildren<Transform> ().GetChild (0).name);

				// One collision might be triggered several times. After removing it after the first collision a it cannot be removed a second time -> an err is thorwn
				try {

					for (int j = 0; j < numberOfSegments; j++) {

						if (j < numberOfSegments - 2) {
							roadSegmentsQueue.Enqueue(roadSegmentsQueue.Dequeue());

						} else {

							Transform segment = roadSegmentsQueue.Dequeue();

							// Name of the segment to remove. Strip the "(Clone)" from the end
							string segmentName = segment.gameObject.name.Substring (0, segment.gameObject.name.Length - 7);

						 	// Find the right roadSegment
							foreach (RoadSegment roadSegment in roadSegments) {
								if (roadSegment.prefab.gameObject.name == segmentName) {
	
									Debug.Log ("Found element: " + roadSegment.prefab.gameObject.name);
	
									// Update (/undo) position and rotation
									float angle = -1 * (segmentStartRotation.y * Mathf.PI / 180);
									Vector3 deltaMove = new Vector3(roadSegment.deltaEndPosition.x * Mathf.Cos(angle) - roadSegment.deltaEndPosition.z * Mathf.Sin(angle), 0, roadSegment.deltaEndPosition.x * Mathf.Sin(angle) + roadSegment.deltaEndPosition.z * Mathf.Cos(angle));
	
//									Debug.Log(segmentStartPosition);
									segmentStartPosition = segmentStartPosition - deltaMove;
									segmentStartRotation = segmentStartRotation - roadSegment.deltaEndRotation;
//									Debug.Log(segmentStartPosition);

									// In order to keep the right numbering
									lastIndex--;
	
									hasTriggered = true;
									break;
								}
							}
	
							Destroy (segment.gameObject);
						}
					}
				} catch {}
			}
		}
	}

}
