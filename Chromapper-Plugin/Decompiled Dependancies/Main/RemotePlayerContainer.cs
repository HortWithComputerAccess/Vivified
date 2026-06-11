using System.Text;
using Discord;
using TMPro;
using UnityEngine;

public class RemotePlayerContainer : MonoBehaviour
{
	public Transform CameraTransform;

	public Transform GridTransform;

	[SerializeField]
	private TextMeshPro nameMesh;

	[SerializeField]
	private TextMeshPro gridNameMesh;

	[SerializeField]
	private SpriteRenderer gridSprite;

	[SerializeField]
	private SpriteRenderer faceSprite;

	private Transform lookAt;

	private MultiNetListener source;

	private MapperIdentityPacket identity;

	private int latency;

	public void AssignIdentity(MultiNetListener source, MapperIdentityPacket identity)
	{
		this.identity = identity;
		this.source = source;
		nameMesh.text = identity.Name;
		SpriteRenderer spriteRenderer = gridSprite;
		Color color = (gridNameMesh.color = identity.Color);
		spriteRenderer.color = color;
		if (identity.DiscordId > 0 && DiscordController.IsActive)
		{
			ImageHandle handle = ImageHandle.User(identity.DiscordId, 128u);
			DiscordController.ImageManager.Fetch(handle, refresh: true, delegate(Result res, ImageHandle updatedHandle)
			{
				Texture2D texture = DiscordController.ImageManager.GetTexture(updatedHandle);
				faceSprite.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), 0.5f * Vector2.one);
				faceSprite.flipY = true;
			});
		}
		UpdateGridText();
	}

	public void UpdateLatency(int latency)
	{
		this.latency = latency;
		UpdateGridText();
	}

	public void Kick()
	{
		if (source is INetAdmin netAdmin)
		{
			netAdmin.Kick(identity);
		}
	}

	public void Ban()
	{
		if (source is INetAdmin netAdmin)
		{
			netAdmin.Ban(identity);
		}
	}

	private void Start()
	{
		lookAt = Camera.main.transform;
	}

	private void Update()
	{
		nameMesh.transform.LookAt(lookAt);
	}

	private void OnDestroy()
	{
		if (faceSprite.flipY)
		{
			Object.Destroy(faceSprite.sprite);
		}
	}

	private void UpdateGridText()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (identity.ConnectionId == 0)
		{
			stringBuilder.Append("<b>[Host]</b> ");
		}
		gridNameMesh.text = stringBuilder.Append(latency).Append("ms\n").Append(identity.Name)
			.ToString();
	}
}
