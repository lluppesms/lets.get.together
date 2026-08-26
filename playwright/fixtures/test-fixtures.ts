import { test as base } from '@playwright/test';
import { HomePage } from '../page-objects/home.page';
import { SearchPage } from '../page-objects/search.page';
import { AboutPage } from '../page-objects/about.page';
import { LayoutComponent } from '../page-objects/layout.component';
import { CirclesPage } from '../page-objects/circles.page';
import { EventsPage } from '../page-objects/events.page';
import { CalendarPage } from '../page-objects/calendar.page';

type SmokeTestFixtures = {
    homePage: HomePage;
    searchPage: SearchPage;
    aboutPage: AboutPage;
    layout: LayoutComponent;
    circlesPage: CirclesPage;
    eventsPage: EventsPage;
    calendarPage: CalendarPage;
};

export const test = base.extend<SmokeTestFixtures>({
    homePage: async ({ page }, use) => {
        await use(new HomePage(page));
    },
    searchPage: async ({ page }, use) => {
        await use(new SearchPage(page));
    },
    aboutPage: async ({ page }, use) => {
        await use(new AboutPage(page));
    },
    layout: async ({ page }, use) => {
        await use(new LayoutComponent(page));
    },
    circlesPage: async ({ page }, use) => {
        await use(new CirclesPage(page));
    },
    eventsPage: async ({ page }, use) => {
        await use(new EventsPage(page));
    },
    calendarPage: async ({ page }, use) => {
        await use(new CalendarPage(page));
    },
});

export { expect } from '@playwright/test';
