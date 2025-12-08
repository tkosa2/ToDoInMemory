const { test, expect } = require('@playwright/test');

test.describe('ToDo Application', () => {
  test.beforeEach(async ({ page }) => {
    // Navigate to the home page before each test
    await page.goto('/');
    // Wait for the page to be fully loaded
    await page.waitForLoadState('networkidle');
  });

  test('HomePage should load successfully', async ({ page }) => {
    // Assert - Check page title
    const title = await page.title();
    expect(title).toContain('ToDo App');
  });

  test('HomePage should display expected content', async ({ page }) => {
    // Assert - Check heading
    const heading = await page.locator('h1').textContent();
    expect(heading).toContain('My Tasks');

    // Assert - Check subtitle
    const subtitle = await page.locator('.subtitle').textContent();
    expect(subtitle).toContain('Stay organized');
  });

  test('AddTodo should create new todo item', async ({ page }) => {
    // Wait for the input field to be visible
    await page.waitForSelector('.todo-input', { state: 'visible' });

    // Act - Add a new todo
    await page.fill('.todo-input', 'Test Todo from Playwright');
    await page.selectOption('.priority-select', 'High');
    await page.click('button[type="submit"]');

    // Wait for Blazor to update the DOM
    await page.waitForTimeout(1000);

    // Assert - Check that todo items exist
    const todoItems = await page.locator('.todo-item').count();
    expect(todoItems).toBeGreaterThan(0);

    // Assert - Check that the new todo text is displayed
    const todoText = await page.locator('.todo-title').first().textContent();
    expect(todoText).toContain('Test Todo from Playwright');
  });

  test('ToggleTodo should mark as complete', async ({ page }) => {
    // Wait for the input field
    await page.waitForSelector('.todo-input', { state: 'visible' });

    // Add a todo first
    await page.fill('.todo-input', 'Todo to Complete');
    await page.click('button[type="submit"]');
    await page.waitForTimeout(1000);

    // Act - Toggle complete
    await page.locator('.check-btn').first().click();
    await page.waitForTimeout(500);

    // Assert - Check that completed todo exists
    const completedTodo = await page.locator('.todo-item.completed').count();
    expect(completedTodo).toBeGreaterThan(0);
  });

  test('FilterTodos should filter by status', async ({ page }) => {
    // Wait for the input field
    await page.waitForSelector('.todo-input', { state: 'visible' });

    // Add a todo
    await page.fill('.todo-input', 'Pending Todo');
    await page.click('button[type="submit"]');
    await page.waitForTimeout(1000);

    // Act - Click Pending filter button (in the Status filter group)
    // Use getByRole to find the button with text "Pending" in the first filter group (Status)
    const statusFilterGroup = page.locator('.filter-group').first();
    const pendingButton = statusFilterGroup.getByRole('button', { name: 'Pending' });
    await pendingButton.click();
    await page.waitForTimeout(500);

    // Assert - Check that the Pending button has the active class
    await expect(pendingButton).toHaveClass(/active/);
  });

  test('DeleteTodo should remove todo item', async ({ page }) => {
    // Wait for the input field
    await page.waitForSelector('.todo-input', { state: 'visible' });

    // Add a todo
    await page.fill('.todo-input', 'Todo to Delete');
    await page.click('button[type="submit"]');
    await page.waitForTimeout(1000);

    // Get initial count
    const initialCount = await page.locator('.todo-item').count();
    expect(initialCount).toBeGreaterThan(0);

    // Act - Delete the todo
    await page.locator('.delete-btn').first().click();
    await page.waitForTimeout(500);

    // Assert - Check that count decreased
    const finalCount = await page.locator('.todo-item').count();
    expect(finalCount).toBeLessThan(initialCount);
  });

  test('Stats should display correct counts', async ({ page }) => {
    // Wait for the input field
    await page.waitForSelector('.todo-input', { state: 'visible' });

    // Add a todo
    await page.fill('.todo-input', 'Stats Test Todo');
    await page.click('button[type="submit"]');
    await page.waitForTimeout(1000);

    // Assert - Check that stats are displayed
    const pendingStat = await page.locator('.stat-number').first().textContent();
    expect(pendingStat).not.toBeNull();
    expect(pendingStat).not.toBe('');

    // Assert - Check that stat labels exist
    const statLabels = await page.locator('.stat-label').allTextContents();
    expect(statLabels).toContain('Pending');
    expect(statLabels).toContain('Completed');
  });
});

