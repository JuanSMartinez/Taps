#pragma once
#include "MotuCore.h"

Handlers::MotuPlayer* player;
std::mutex* mutex;
FinishedPlayingCallback Handler;


void AsyncSinePlay(void*) 
{
	if (mutex->try_lock())
	{
		player->changePlayMode(Handlers::MotuPlayer::sine);
		PaError initResult = Pa_Initialize();
		if (initResult != paNoError) goto error;
		if (player->isUsingMotu())
		{
			if (!player->setMOTU()) goto error;
		}
		else
		{
			if (!player->setDefaultOutput()) goto error;
		}

		if (player->openStream())
		{
			if (player->startStream())
			{
				Pa_Sleep(2000);
				player->stopStream();
			}
			player->closeStream();
		}
		player->restartControlVariables();
		Pa_Terminate();
		mutex->unlock();
		Handler(0);
		_endthread();
	}
	else
		_endthread();
	
error:
	Handler(-1);
	mutex->unlock();
	Pa_Terminate();
	_endthread();
}

void AsyncPhonemePlay(void* phonemeCode)
{
	if (mutex->try_lock())
	{
		int code = (int)phonemeCode;
		player->setPhonemeIndex(code);
		player->changePlayMode(Handlers::MotuPlayer::phoneme);
		PaError initResult = Pa_Initialize();
		if (initResult != paNoError) goto error;
		if (player->isUsingMotu())
		{
			if (!player->setMOTU()) goto error;
		}
		else
		{
			if (!player->setDefaultOutput()) goto error;
		}
		if (player->openStream())
		{
			if (player->startStream())
			{
				while (player->isStreamActive());
				player->stopStream();
			}
			player->closeStream();
		}
		player->restartControlVariables();
		Pa_Terminate();
		mutex->unlock();
		Handler(0);
		_endthread();
	}
	else 
	{
		_endthread();
	}
error:
	Handler(-1);
	mutex->unlock();
	Pa_Terminate();
	_endthread();
}


void AsyncMatrixPlay(void*)
{
	if (mutex->try_lock())
	{
		PaError initResult = Pa_Initialize();
		if (initResult != paNoError) goto error;
		if (player->isUsingMotu())
		{
			if (!player->setMOTU()) goto error;
		}
		else
		{
			if (!player->setDefaultOutput()) goto error;
		}
		if (player->openStream())
		{
			if (player->startStream())
			{
				while (player->isStreamActive());
				player->stopStream();
			}
			player->closeStream();
		}
		player->restartControlVariables();
		Pa_Terminate();
		mutex->unlock();
		Handler(0);
		_endthread();
	}
	else
		_endthread();
error:
	Handler(-1);
	Pa_Terminate();
	mutex->unlock();
	_endthread();
}


/*Test a sine wave in all 24 channels*/
DLLEXPORT void testPlay()
{
	
	_beginthread(AsyncSinePlay, 0, NULL);

}

DLLEXPORT void play(int phonemeCode)
{
	_beginthread(AsyncPhonemePlay, 0, (void*)phonemeCode);
	
}

//Play a specific matrix 
DLLEXPORT void playMatrix(float* matrix, int width, int height)
{
	player->changePlayMode(Handlers::MotuPlayer::matrix);
	player->setArbitraryMatrixParameters(matrix, width, height);
	_beginthread(AsyncMatrixPlay, 0, NULL);

}

/*Create necessary data structures*/
DLLEXPORT void createStructures()
{
	player = new Handlers::MotuPlayer();
	mutex = new std::mutex;
	Handler = 0;
}

//Return the log code
DLLEXPORT int getLogCode()
{
	return 0;
}

//Is the device playing
DLLEXPORT int isMotuPlaying()
{
	return player->isPlaying();
}

//Use motu as the playback device
DLLEXPORT void useMotu()
{
	player->useMotu();
}

//Use the default device as the playback device
DLLEXPORT void useDefaultOutput()
{
	player->useDefault();
}

//Set a finished playing callback
DLLEXPORT void setFinishedPlayingCallback(FinishedPlayingCallback handler)
{
	Handler = handler;
}


