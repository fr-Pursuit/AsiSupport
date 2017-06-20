#include "Stdafx.h"

void AsiSupport::TexturesManager::Initialize()
{
	Rage::Game::RawFrameRender += gcnew System::EventHandler<Rage::GraphicsEventArgs ^>(&AsiSupport::TexturesManager::Draw);
}

int AsiSupport::TexturesManager::CreateTexture(String^ fileName)
{
	textures->Add(idCount, Rage::Game::CreateTextureFromFile(fileName));
	idCount++;
	return idCount - 1;
}

void AsiSupport::TexturesManager::DrawTexture(int id, int time, float x, float y, float width, float height, float rotation, float rotationCenterX, float rotationCenterY)
{
	calls->Add(gcnew DrawCall(id, time, x, y, width, height, rotation, rotationCenterX, rotationCenterY));
}

void AsiSupport::TexturesManager::Draw(Object ^ sender, Rage::GraphicsEventArgs ^ args)
{
	for(int i = 0; i < calls->Count; i++)
	{
		DrawCall^ call = calls[i];

		if(Environment::TickCount <= call->ValidUntil)
		{
			Rage::Vector2 position = Scale(call->RenderX, call->RenderY);
			Rage::Vector2 size = Scale(call->Width, call->Height);
			Rage::Vector2 rotationCenter = Rage::Vector2(position.X + (size.X * call->RotationCenterX), position.Y + (size.Y * call->RotationCenterY));
			args->Graphics->DrawTexture(textures[call->TextureId], position, size, 0.0f, 0.0f, 1.0f, 1.0f, call->Rotation, rotationCenter);
		}
		else
		{
			calls->Remove(call);
			i--;
		}
	}
}

Rage::Vector2 AsiSupport::TexturesManager::Scale(float x, float y)
{
	return Rage::Vector2(x * Rage::Game::Resolution.Width, y * Rage::Game::Resolution.Height);
}