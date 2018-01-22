using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockController : MonoBehaviour
{
	public float explosionPower = 600f;
	public float explosionRadius = 2f;
	public float explosionOffset = 2f;
	public float explosionDelay = 2f;
	public float expolsionFlashRate = 0.1f;
	public AudioClip explosionSound;
	public AudioClip safeSound;
	public AudioClip flagSound;
	public AudioClip markSound;

	private GameController game;

	private int xNum;
	private int yNum;
	private int zNum;

	private List<BlockController> neighbors;
	private BlockState state;

	private int minesNearBy;
	private bool isMine;

	private Transform cube;
	private Transform text;

	private Renderer cubeRenderer;
	private Renderer textRenderer;

	private Rigidbody rigidBody;
	private Collider collider;
	private TextMesh textMesh;

	void Start()
	{

	}

	void Update()
	{
		if (state != BlockState.HIDDEN) {
			Quaternion rotation = text.transform.rotation;
			rotation.SetLookRotation(GameController.mainCamera.transform.forward, GameController.mainCamera.transform.up);
			text.transform.rotation = rotation;
		}
	}

	public void Init(GameController gameController, int xNum, int yNum, int zNum)
	{
		this.game = gameController;

		this.xNum = xNum;
		this.yNum = yNum;
		this.zNum = zNum;

		cube = transform.Find("Cube");
		text = transform.Find("Text");

		cubeRenderer = cube.GetComponent<Renderer>();
		textRenderer = text.GetComponent<Renderer>();

		rigidBody = GetComponent<Rigidbody>();
		rigidBody.Sleep();

		collider = GetComponent<Collider>();

		textMesh = text.GetComponent<TextMesh>();
		textMesh.fontSize = 500;
		textMesh.alignment = TextAlignment.Center;
		textMesh.anchor = TextAnchor.MiddleCenter;

		SetState(BlockState.HIDDEN);
	}

	public void SetNeighbors(List<BlockController> neighbors)
	{
		this.neighbors = neighbors;
		minesNearBy = 0;

		for (int i = 0; i < neighbors.Count; i++) {
			if (neighbors[i].IsMine()) {
				minesNearBy += 1;
			}
		}
	}

	public void SetMine(bool isMine)
	{
		this.isMine = isMine;
	}

	public void SetState(BlockState state)
	{
		this.state = state;

		switch (this.state) {
			case BlockState.HIDDEN:
				OnHidden();
				break;
			case BlockState.FLAGGED:
				OnFlagged();
				break;
			case BlockState.MARKED:
				OnMarked();
				break;
			case BlockState.MINED:
				OnMined();
				break;
		}
	}

	public BlockState GetState()
	{
		return state;
	}

	public bool IsMine()
	{
		return isMine;
	}

	public int[] GetXyzNum()
	{
		return new int[] { xNum, yNum, zNum };
	}

	public void PrintNeighbor()
	{
		print("Center Block : [x=" + xNum + ",y=" + yNum + ",z=" + zNum + "]");
		print("Number of mines" + minesNearBy);
		for (int i = 0; i < neighbors.Count; i++) {
			int x = neighbors[i].GetXyzNum()[0];
			int y = neighbors[i].GetXyzNum()[1];
			int z = neighbors[i].GetXyzNum()[2];
			print("\tNeighbor [" + i + "] : [x=" + x + ",y=" + y + ",z=" + z + ",isMine=" + neighbors[i].IsMine() + "]");
		}
	}

	private void OnHidden()
	{
		cube.gameObject.SetActive(true);
		text.gameObject.SetActive(false);

		cube.parent.gameObject.layer = LayerMask.NameToLayer("CubeBlock");

		text.transform.localEulerAngles = Vector3.zero;

		cubeRenderer.material = AppearanceManager.GetBlockSurfaceMaterial();
	}

	private void OnFlagged()
	{
		game.OnBlockFlagged(this);

		cube.gameObject.SetActive(false);
		text.gameObject.SetActive(true);

		AudioSource.PlayClipAtPoint(flagSound, transform.position);

		cube.parent.gameObject.layer = LayerMask.NameToLayer("TextBlock");

		textMesh.text = "!";
		textMesh.font = AppearanceManager.GetFont();
		textMesh.fontStyle = AppearanceManager.GetFontStyle();

		textRenderer.material = AppearanceManager.GetTextMaterial();
		textRenderer.material.color = AppearanceManager.GetTextColor("Flag");

		text.localScale = new Vector3(0.025f, 0.025f, 0.025f);
	}

	private void OnMarked()
	{
		game.OnBlockDeflagged(this);

		cube.gameObject.SetActive(false);
		text.gameObject.SetActive(true);

		AudioSource.PlayClipAtPoint(markSound, transform.position);

		cube.parent.gameObject.layer = LayerMask.NameToLayer("TextBlock");

		textMesh.text = "?";
		textMesh.font = AppearanceManager.GetFont();
		textMesh.fontStyle = AppearanceManager.GetFontStyle();

		textRenderer.material = AppearanceManager.GetTextMaterial();
		textRenderer.material.color = AppearanceManager.GetTextColor("Mark");

		text.localScale = new Vector3(0.025f, 0.025f, 0.025f);
	}

	private void OnMined()
	{
		cube.gameObject.SetActive(false);
		text.gameObject.SetActive(true);

		cube.parent.gameObject.layer = LayerMask.NameToLayer("MinedBlock");

		if (isMine) {
			textMesh.text = "X";
			textMesh.font = AppearanceManager.GetFont();
			textMesh.fontStyle = AppearanceManager.GetFontStyle();

			textRenderer.material = AppearanceManager.GetTextMaterial();
			textRenderer.material.color = AppearanceManager.GetTextColor("Mine");
			text.localScale = new Vector3(0.03f, 0.03f, 0.03f);

			StartCoroutine(Explode());
		} else {
			if (minesNearBy > 0) {
				textMesh.text = minesNearBy.ToString();
			} else {
				textMesh.text = "";
				foreach (BlockController neighbor in neighbors) {
					if (neighbor.GetState() != BlockState.MINED) {
						neighbor.SetState(BlockState.MINED);
					}
				}
			}

			AudioSource.PlayClipAtPoint(safeSound, transform.position);

			textMesh.font = AppearanceManager.GetFont();
			textMesh.fontStyle = AppearanceManager.GetFontStyle();

			textRenderer.material = AppearanceManager.GetTextMaterial();
			//textRenderer.material.color = AppearanceManager.GetTextColor(minesNearBy);
			textRenderer.material.color = AppearanceManager.GetTextColor(minesNearBy);
			if (minesNearBy < 10) {
				text.localScale = new Vector3(0.025f, 0.025f, 0.025f);
			} else {
				text.localScale = new Vector3(0.018f, 0.018f, 0.018f);
			}
		}

		game.OnBlockMined(this);
	}

	public IEnumerator Explode()
	{
		float duration = explosionDelay;
		while (duration > 0) {
			text.gameObject.SetActive(!text.gameObject.activeSelf);
			duration -= Time.deltaTime;
			yield return expolsionFlashRate;
		}
		text.gameObject.SetActive(true);
		AudioSource.PlayClipAtPoint(explosionSound, transform.position);
		//yield return new WaitForSeconds(explosionDelay);
		Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
		foreach (Collider collider in colliders) {
			if (collider.name.Contains("Block")) {

				BlockController controller = collider.GetComponent<BlockController>();
				if (controller.GetState() != BlockState.MINED && controller.IsMine()) {
					controller.SetState(BlockState.MINED);
				}

				collider.GetComponent<Rigidbody>().AddExplosionForce(UnityEngine.Random.Range(explosionPower * 0.5f, explosionPower * 1.5f), transform.position, explosionRadius, UnityEngine.Random.Range(-explosionOffset, explosionOffset));
			}
		}
		float distance = (transform.position - GameController.mainCamera.transform.position).magnitude;
		game.StartCoroutine(game.ExeShakeTask(distance));
		gameObject.SetActive(false);
	}

	public void OnNeighborDestroyed(BlockController neighbor)
	{
		if (neighbors.Contains(neighbor)) {
			neighbors.Remove(neighbor);
		}
	}

	public void OnDestroyed()
	{
		for (int i = 0; i < neighbors.Count; i++) {
			neighbors[i].OnNeighborDestroyed(this);
		}

		Destroy(gameObject);
	}
}