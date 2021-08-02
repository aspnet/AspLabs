import { Component } from '@angular/core';

@Component({
  selector: 'app-root',
  template: `
    <div class="content">
      <h1>
        Welcome to {{title}}!
      </h1>
      <span style="display: block">{{ title }} app is running!</span>
    </div>
    <my-counter increment-amount="2"></my-counter>
  `,
  styles: []
})
export class AppComponent {
  title = 'angular-app-with-blazor';
}
