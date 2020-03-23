package com.companyname.weather.services

import android.content.Context
import android.location.Location
import android.os.Handler
import android.os.Looper
import androidx.concurrent.futures.CallbackToFutureAdapter
import androidx.lifecycle.MutableLiveData
import androidx.work.*
import com.google.common.util.concurrent.ListenableFuture
import kotlinx.coroutines.GlobalScope
import kotlinx.coroutines.launch
import kotlinx.coroutines.sync.Mutex
import kotlinx.coroutines.sync.withLock
import java.util.*
import java.util.concurrent.TimeUnit

class RefreshService {

    companion object {
        val refresh: MutableLiveData<Context> = MutableLiveData()

        private lateinit var handler: Handler
        private lateinit var runnable: Runnable
    }

    private val refreshInterval: Long = 1000 * 60 * 15
//    private val refreshInterval: Long = 1000 * 15

    fun resume(context: Context) {
        handler = Handler()
        runnable = Runnable {
            refresh.postValue(context)
            handler.postDelayed(runnable, refreshInterval)
        }
        handler.postDelayed(runnable, refreshInterval)
    }

    fun pause() {
        handler.removeCallbacks(runnable)
    }
}