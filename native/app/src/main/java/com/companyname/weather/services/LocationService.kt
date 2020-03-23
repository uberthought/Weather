package com.companyname.weather.services

import android.Manifest
import android.content.Context
import android.content.pm.PackageManager
import android.location.Location
import android.os.Looper
import androidx.core.app.ActivityCompat
import androidx.lifecycle.MutableLiveData
import com.companyname.weather.MainActivity
import com.companyname.weather.fragments.PreferencesFragment
import com.google.android.gms.location.*

class LocationService() {

    companion object {
        val lastLocation: MutableLiveData<Location> = MutableLiveData()

        private val observers: Observers = Observers()
        private var locationCallback: LocationCallback? = null
        private var fusedLocationClient: FusedLocationProviderClient? = null
    }

    private class Observers() {
        init {
            PreferencesFragment.lastLocation.observeForever { location ->
                location ?: return@observeForever

                if (lastLocation.value == null || lastLocation.value!!.latitude != location.latitude || lastLocation.value!!.longitude != location.longitude)
                    lastLocation.value = location
            }

            RefreshService.refresh.observeForever {context ->
                LocationService().resume(context)
            }
        }
    }

//    private val granularity = arrayOf(Manifest.permission.ACCESS_FINE_LOCATION, Manifest.permission.ACCESS_COARSE_LOCATION)
    private val granularity = arrayOf(Manifest.permission.ACCESS_COARSE_LOCATION)

    init {
        if (lastLocation.value == null)
            PreferencesFragment.lastLocation.value?.let { setLocation(it) }
    }

    fun setLocation(location: Location) {
        if (lastLocation.value == null || lastLocation.value!!.latitude != location.latitude || lastLocation.value!!.longitude != location.longitude)
            lastLocation.value = location
    }

    fun requestPermissions(activity: MainActivity) {
        PreferencesFragment.useDevice.value?.let {
            if (!it)
                return
        }

        if (granularity.any { l -> ActivityCompat.checkSelfPermission(activity.applicationContext, l) != PackageManager.PERMISSION_GRANTED }) {
            ActivityCompat.requestPermissions(activity, granularity, MainActivity.LOCATION_REQUEST)
            return
        }
    }

    fun resume(context: Context) {
        if (granularity.any { l -> ActivityCompat.checkSelfPermission(context, l) != PackageManager.PERMISSION_GRANTED })
            return

        fusedLocationClient = LocationServices.getFusedLocationProviderClient(context)
        fusedLocationClient!!.lastLocation.addOnSuccessListener { setLocation(it) }

        val locationRequest = LocationRequest.create()?.apply {
            this.interval = 1000
            this.fastestInterval = 1000
            this.priority = LocationRequest.PRIORITY_HIGH_ACCURACY
        }

        locationCallback = object : LocationCallback() {
            override fun onLocationResult(locationResult: LocationResult?) {
                locationResult?.let {
                    val lastLocation = locationResult.locations.last()
                    lastLocation?.let { newLocation -> setLocation(newLocation) }
                    fusedLocationClient!!.removeLocationUpdates(locationCallback)
                }
            }
        }
        fusedLocationClient!!.requestLocationUpdates(locationRequest, locationCallback, Looper.getMainLooper())
    }

    fun pause() {
        fusedLocationClient ?: return
        locationCallback ?: return

        fusedLocationClient!!.removeLocationUpdates(locationCallback)
    }
}