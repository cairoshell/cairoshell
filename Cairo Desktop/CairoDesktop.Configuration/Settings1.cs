namespace CairoDesktop.Configuration.Properties {
    
    
    // Questa classe consente di gestire eventi specifici sulla classe delle impostazioni:
    //  L'evento SettingChanging viene generato prima della modifica del valore di un'impostazione.
    //  L'evento PropertyChanged viene generato dopo la modifica del valore di un'impostazione.
    //  L'evento SettingsLoaded viene generato dopo il caricamento dei valori dell'impostazione.
    //  L'evento SettingsSaving viene generato prima del salvataggio dei valori dell'impostazione.
    internal sealed partial class Settings {
        
        public Settings() {
            // // Per aggiungere gestori eventi per il salvataggio e la modifica delle impostazioni, rimuovere il commento dalle righe seguenti:
            //
            // this.SettingChanging += this.SettingChangingEventHandler;
            //
            // this.SettingsSaving += this.SettingsSavingEventHandler;
            //
        }
        
        private void SettingChangingEventHandler(object sender, System.Configuration.SettingChangingEventArgs e) {
            // Aggiungere qui il codice per gestire l'evento SettingChangingEvent.
        }
        
        private void SettingsSavingEventHandler(object sender, System.ComponentModel.CancelEventArgs e) {
            // Aggiungere qui il codice per gestire l'evento SettingsSaving.
        }
    }
}
