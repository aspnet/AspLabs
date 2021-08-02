import { Component } from '@angular/core';

@Component({
  selector: 'app-root',
  template: `
    <div class="content">
      <h1>Welcome to {{title}}!</h1>
      <p>This is an Angular application that can also host Blazor components.</p>
      <p>
        <button (click)="addBlazorCounter()">Add Blazor counter</button>
        <button (click)="removeBlazorCounter()">Remove Blazor counter</button>
      </p>

      <div *ngFor="let blazorCounter of blazorCounters; let myIndex = index">
        <my-counter [attr.increment-amount]="myIndex + 1"></my-counter>
      </div>
    </div>
  `,
  styles: []
})
export class AppComponent {
  title = 'angular-app-with-blazor';

  blazorCounters: any[] = [];

  addBlazorCounter() {
    this.blazorCounters.push({});
  }

  removeBlazorCounter() {
    this.blazorCounters.pop();
  }
}
