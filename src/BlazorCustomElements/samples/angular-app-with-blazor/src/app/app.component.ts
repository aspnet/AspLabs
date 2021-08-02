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

      <div *ngFor="let counter of blazorCounters">
        <my-counter [attr.title]="counter.title"
                    [attr.increment-amount]="counter.incrementAmount">
        </my-counter>
      </div>
    </div>
  `,
  styles: []
})
export class AppComponent {
  title = 'angular-app-with-blazor';

  blazorCounters: any[] = [];
  nextCounterIndex = 1;

  addBlazorCounter() {
    const index = this.nextCounterIndex++;
    this.blazorCounters.push({
      title: `Counter ${index}`,
      incrementAmount: index,
    });
  }

  removeBlazorCounter() {
    this.blazorCounters.pop();
  }
}
