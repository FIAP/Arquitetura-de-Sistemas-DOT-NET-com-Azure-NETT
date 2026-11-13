import { enableProdMode, provideZoneChangeDetection } from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
import { defineCustomElements } from '@vime/core';

import { AppModule } from './app/app.module';
import { environment } from './environments/environment';

// Registra os web components do player de vídeo (<vm-player>, <vm-video>...)
defineCustomElements();

if (environment.production) {
  enableProdMode();
}

platformBrowserDynamic().bootstrapModule(AppModule, { applicationProviders: [provideZoneChangeDetection()], })
  .catch(err => console.error(err));
