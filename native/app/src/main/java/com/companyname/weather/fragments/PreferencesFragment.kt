package com.companyname.weather.fragments

import android.content.Context
import android.location.Geocoder
import android.location.Location
import android.os.Bundle
import androidx.lifecycle.MutableLiveData
import androidx.preference.*
import com.companyname.weather.MainActivity
import com.companyname.weather.R

class PreferencesFragment : PreferenceFragmentCompat() {

    companion object {
        val useDevice: MutableLiveData<Boolean> = MutableLiveData()
        val lastLocation: MutableLiveData<Location> = MutableLiveData()

        fun update(context: Context) {
            val manager = PreferenceManager.getDefaultSharedPreferences(context)

            manager.getString("device_location", null)?.let {
                if (!manager.getBoolean("use_device", false)) {
                    LocationUtilities.stringToLocation(context, it)?.let {
                            location -> lastLocation.value = location
                    }
                }
            }

            useDevice.value = manager.getBoolean("use_device", false)
        }

        fun setUseDevice(context: Context, value: Boolean) {
            val manager = PreferenceManager.getDefaultSharedPreferences(context)

            val oldValue = manager.getBoolean("use_device", false)
            if (oldValue == value)
                return

            with(manager.edit()) {
                putBoolean("use_device", value)
                commit()
            }
            useDevice.value = value
        }
    }

    override fun onCreatePreferences(savedInstanceState: Bundle?, rootKey: String?) {
        setPreferencesFromResource(R.xml.preferences, rootKey)

        findPreference<EditTextPreference>("device_location")?.setOnBindEditTextListener { it.setSingleLine() }

        val useDevice = findPreference<SwitchPreferenceCompat>("use_device")
        val deviceLocation = findPreference<EditTextPreference>("device_location")

        useDevice?.let {
            deviceLocation?.isVisible = !it.isChecked
            it.setOnPreferenceChangeListener { _, newValue ->
                onUseDeviceChanged(newValue as Boolean)
                deviceLocation?.isVisible = !newValue
                true
            }
        }

        deviceLocation?.let {
            if (it.text != null)
                it.summary = it.text
            it.setOnPreferenceChangeListener { _, newValue ->
                onDeviceLocationChanged(newValue.toString())
                true
            }
        }
    }

    private fun onUseDeviceChanged(newValue: Boolean) {
        val deviceLocation = findPreference<EditTextPreference>("device_location")
        deviceLocation?.isVisible = !newValue
        useDevice.value = newValue

        deviceLocation?.text?.let { text ->
            val location = LocationUtilities.stringToLocation(context!!, text)
            lastLocation.value = location
        }
    }

    private fun onDeviceLocationChanged(newValue: String) {
        val deviceLocation = findPreference<EditTextPreference>("device_location")
        deviceLocation?.summary = newValue

        val location = LocationUtilities.stringToLocation(context!!, newValue)
        location ?: return
        lastLocation.value = location
    }

}

class LocationUtilities {
    companion object {
        fun stringToLocation(context: Context, value: String): Location? {
            val numbers = value.split(',').map { it.toDoubleOrNull() }
            if (numbers.count() == 2 && !numbers.any() { (it == null) }) {
                with(Location("")) {
                    latitude = numbers[0]!!
                    longitude = numbers[1]!!
                    return this
                }
            }

            val geocoder = Geocoder(context)
            val addresses = geocoder.getFromLocationName(value, 1)
            if (addresses.count() > 0) {
                with (Location("")) {
                    latitude = addresses[0].latitude
                    longitude = addresses[0].longitude
                    return this
                }
            }

            return null
        }
    }
}