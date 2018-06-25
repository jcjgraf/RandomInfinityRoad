using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadGenerator : MonoBehaviour {

	public Transform car;

	public RoadSegment[] roadSegments;

	private Vector3 segmentStartPosition;
	private Vector3 segmentStartRotation;

	private Queue<Transform> roadSegmentsQueue;

	private int numberOfSegments;

	private int lastIndex;

	void Awake () {

		roadSegmentsQueue = new Queue<Transform>();
		segmentStartPosition = new Vector3(0, 0, 0);
		segmentStartRotation = new Vector3(0, 0, 0);

		numberOfSegments = 10;
		lastIndex = 0;

		for (int i = 0; i < numberOfSegments; i++) {

			addSegment();
		}
	}
	
	void Update () {

		int firstSegment;
		int.TryParse(roadSegmentsQueue.ToArray()[0].gameObject.GetComponentInChildren<Transform>().GetChild(0).name, out firstSegment);

		if (getCurrentSegment() > firstSegment + 1) {
			Debug.Log("Left first segment");

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

		// int randInt = Random.Range(0, roadSegments.Length)
		int randInt = Random.Range(0, 5);

		RoadSegment newRoadSegment;

		if (randInt > roadSegments.Length - 1) {
			newRoadSegment = roadSegments[0];

		} else {
			newRoadSegment = roadSegments[randInt];
		}

		GameObject newSegment = Instantiate<GameObject>(newRoadSegment.prefab);

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
	}
}
