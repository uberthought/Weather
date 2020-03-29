package com.companyname.weather.services

import android.content.SharedPreferences
import android.location.Location
import androidx.lifecycle.LifecycleService
import androidx.lifecycle.MutableLiveData
import androidx.lifecycle.Observer
import androidx.preference.PreferenceManager
import com.companyname.weather.R

class PreferenceService : LifecycleService() {
    companion object {
        var instance: PreferenceService? = null
        val useDevice: MutableLiveData<Boolean> = MutableLiveData()
        val location: MutableLiveData<Location> = MutableLiveData()
        val locationName: MutableLiveData<String> = MutableLiveData()
    }

    private lateinit var sharedPreferences: SharedPreferences

    override fun onCreate() {
        super.onCreate()
        instance = this

        sharedPreferences = PreferenceManager.getDefaultSharedPreferences(baseContext)

        useDevice.value = sharedPreferences.getBoolean("use_device", true)
        locationName.value = sharedPreferences.getString("location_name", getString(R.string.not_set))
        location.value = with(Location("")) {
            latitude = sharedPreferences.getFloat("latitude", 0f).toDouble()
            longitude = sharedPreferences.getFloat("longitude", 0f).toDouble()
            this
        }

        useDevice.observe(this, Observer { useDevice ->
            if (useDevice == sharedPreferences.getBoolean("use_device", false))
                return@Observer
            with(sharedPreferences.edit()) {
                putBoolean("use_device", useDevice)
                apply()
            }
        })

        location.observe(this, Observer { location ->
            val latitude = sharedPreferences.getFloat("latitude", 0f)
            val longitude = sharedPreferences.getFloat("longitude", 0f)

            if (location.latitude.toFloat() == latitude && location.longitude.toFloat() == longitude)
                return@Observer
            with(sharedPreferences.edit()) {
                putFloat("latitude", location.latitude.toFloat())
                putFloat("longitude", location.longitude.toFloat())
                apply()
            }
        })

        locationName.observe(this, Observer { locationName ->
            if (locationName == sharedPreferences.getString("location_name", ""))
                return@Observer
            with(sharedPreferences.edit()) {
                putString("location_name", locationName)
                apply()
            }
        })

        LocationService.location.observe(this, Observer { location ->
            if (useDevice.value == true) {
                PreferenceService.location.value = location
                locationName.value =
                    if (location.latitude != 0.0 || location.longitude != 0.0) "${location.latitude}, ${location.longitude}"
                    else getString(R.string.not_set)
            }
        })
    }

    override fun onDestroy() {
        super.onDestroy()
        instance = null
    }
}