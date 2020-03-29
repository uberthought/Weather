package com.companyname.weather.services

import android.Manifest
import android.content.pm.PackageManager
import android.location.Location
import android.os.Looper
import androidx.core.app.ActivityCompat
import androidx.lifecycle.LifecycleService
import androidx.lifecycle.MutableLiveData
import androidx.lifecycle.Observer
import com.companyname.weather.MainActivity
import com.google.android.gms.location.*

class LocationService : LifecycleService() {

    companion object {
        var instance: LocationService? = null
        val location: MutableLiveData<Location> = MutableLiveData(Location(""))
        val hasPermission: MutableLiveData<Boolean> = MutableLiveData(false)

        val granularity = arrayOf(Manifest.permission.ACCESS_FINE_LOCATION, Manifest.permission.ACCESS_COARSE_LOCATION)
    }

    private val interval: Long = 1000 * 60 * 15
//    private val interval: Long = 1000 * 15

    private var locationCallback: LocationCallback? = null

    override fun onCreate() {
        super.onCreate()
        instance = this

        PreferenceService.location.observe(this, Observer { location ->
            if (PreferenceService.useDevice.value != false)
                return@Observer
            with(Companion.location) {
                if (value == null)
                    value = location
                if (value!!.latitude != location.latitude || value!!.longitude != location.longitude)
                    value = location
            }
        })

        PreferenceService.useDevice.observe(this, Observer { useDevice ->
            if (useDevice)
                requestLocationUpdates()
            else
                cancelLocationUpdates()
        })
    }

    override fun onDestroy() {
        super.onDestroy()
        instance = null
    }

    fun requestLocationUpdates() {
        if (PreferenceService.useDevice.value != true)
            return

        MainActivity.instance?.let { activity ->
            if (!granularity.all { l -> ActivityCompat.checkSelfPermission(baseContext, l) == PackageManager.PERMISSION_GRANTED }) {
                ActivityCompat.requestPermissions(activity, granularity, MainActivity.LOCATION_REQUEST)
                return
            }
        }

        if (!granularity.all { l -> ActivityCompat.checkSelfPermission(baseContext, l) == PackageManager.PERMISSION_GRANTED })
            return

        hasPermission.value?.let {
            if (!it)
                hasPermission.value = true
        }

        val fusedLocationClient = LocationServices.getFusedLocationProviderClient(baseContext)

        fusedLocationClient.lastLocation.addOnSuccessListener { it?.let { location.value = it } }

        val locationRequest = LocationRequest.create()?.apply {
            this.interval = this@LocationService.interval
            this.fastestInterval = this@LocationService.interval
            this.priority = LocationRequest.PRIORITY_HIGH_ACCURACY
        }

        if (locationCallback == null)
            locationCallback = object : LocationCallback() {
                override fun onLocationResult(locationResult: LocationResult?) {
                    locationResult?.let { locationResult ->
                        val location = locationResult.locations.last()
                        with(Companion.location) {
                            if (value == null)
                                value = location
                            if (value!!.latitude != location.latitude || value!!.longitude != location.longitude)
                                value = location
                        }
                    }
                }
        }
        fusedLocationClient.requestLocationUpdates(locationRequest, locationCallback, Looper.getMainLooper())
    }

    private fun cancelLocationUpdates() {
        locationCallback ?: return
        val fusedLocationClient = LocationServices.getFusedLocationProviderClient(baseContext)
        fusedLocationClient.removeLocationUpdates(locationCallback)
        locationCallback = null
    }
}