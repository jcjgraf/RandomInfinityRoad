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

	void Awake () {

		TriggerManager.colliderTriggered += trigger;

		roadSegmentsQueue = new Queue<Transform>();
		segmentStartPosition = new Vector3(0, 0, 0);
		segmentStartRotation = new Vector3(0, 0, 0);

		numberOfSegments = 7;
		lastIndex = 0;

		for (int i = 0; i < numberOfSegments; i++) {

			addSegment();
		}
	}
	
	void Update () {
		/*
			Get the name resp. its number of the first segment in the roadSegmentsQueme and compare it the with segment the car is on currently. If it has passed the first segment, destroy it and generate a new one.
		*/

		int firstSegment;
		int.TryParse(roadSegmentsQueue.ToArray()[0].gameObject.GetComponentInChildren<Transform>().GetChild(0).name, out firstSegment);

		if (getCurrentSegment() > firstSegment + 1) {

			Transform toRemove = roadSegmentsQueue.Dequeue();
			Destroy(toRemove.gameObject);

			addSegment();
		}

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

	void addSegment() {

		// int randInt = Random.Range(0, roadSegments.Length);
		int randInt = Random.Range(0, roadSegments.Length);

		RoadSegment newRoadSegment;

		if (randInt > roadSegments.Length - 1) {
			// Ugly way to generate more straight segments
			newRoadSegment = roadSegments[0];

		} else {
			newRoadSegment = roadSegments[randInt];
		}

		GameObject newSegment = Instantiate<GameObject>(newRoadSegment.prefab);

//TODO validate position

		bool call = isValidPosition(newSegment, segmentStartPosition + newRoadSegment.standardPosition, segmentStartRotation + newRoadSegment.standardRotation, newRoadSegment.standardScale);




		// Transform new gameObject
		newSegment.transform.position = segmentStartPosition + newRoadSegment.standardPosition;
		newSegment.transform.eulerAngles = segmentStartRotation + newRoadSegment.standardRotation;
		newSegment.transform.localScale = newRoadSegment.standardScale;

		newSegment.gameObject.GetComponentInChildren<Transform>().GetChild(0).name = lastIndex.ToString();

		lastIndex++;

		// Update position and rotation
		float angle = -1 * (segmentStartRotation.y * Mathf.PI / 180);  // The -1 is needed since the rotation matrix works in the other way around as the unity rotation system
		// Apply a rotation vectory
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

	bool isValidPosition(GameObject newSegment, Vector3 newPosition, Vector3 newRotation, Vector3 newScale) {

		// Setup Rigidbody
		Rigidbody newSegementRigidbody =  newSegment.AddComponent<Rigidbody>();
		newSegementRigidbody.isKinematic = true;

		// Add the class with the triggerEvent to the newly created gameObject
		// TriggerManager triggerScript = newSegment.AddComponent(typeof(TriggerManager)) as TriggerManager;

		newSegment.AddComponent(typeof(TriggerManager));

		// Add collider and set right properties
		MeshCollider newSegmentCollider = newSegment.gameObject.GetComponentInChildren<Transform>().GetChild(0).GetComponent<MeshCollider>();

		newSegmentCollider.convex = true;
		newSegmentCollider.isTrigger = true;

		// Set segment to position
		newSegment.transform.position = newPosition;
		newSegment.transform.eulerAngles = newRotation;
		newSegment.transform.localScale = newScale;

		// Check that collision is not with the previous road segment (overlapping)

		// Get trigger event and check for name

		return false;

	}

	void removeTriggerDetection(GameObject segment) {

		// Undo changes
		segment.gameObject.GetComponentInChildren<Transform>().GetChild(0).GetComponent<MeshCollider>().isTrigger = false;
		segment.gameObject.GetComponentInChildren<Transform>().GetChild(0).GetComponent<MeshCollider>().convex = false;
		Destroy(segment.GetComponent<Rigidbody>());
		// Destroy(newSegementRigidbody);
		// Destroy(triggerScript);
		Destroy(segment.GetComponent<TriggerManager>());

	}

	void trigger() {
		
		// Debug.Log("Event received");
	}

}
