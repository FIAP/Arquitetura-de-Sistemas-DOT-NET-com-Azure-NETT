import { provideHttpClient, withInterceptorsFromDi, withXhr } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { HeaderComponent } from './pages/components/header/header.component';
import { HomeModule } from './pages/home/home.module';
import { HomeService } from './pages/home/services/home.service';

@NgModule({ declarations: [
        AppComponent,
        HeaderComponent
    ],
    bootstrap: [AppComponent], imports: [BrowserModule,
        HomeModule,
        AppRoutingModule], providers: [HomeService, provideHttpClient(withXhr(), withInterceptorsFromDi())] })
export class AppModule { }
