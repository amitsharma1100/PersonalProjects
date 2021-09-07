<?xml version="1.0"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="html" encoding="UTF-8"/>
  <xsl:template match="//InvoiceData">
    <html>
      <Report>
        <Title>Invoice</Title>
      </Report>
      <body>
        <table class="tg" style="border-collapse: collapse; border-spacing: 0; width: 100%">
          <tr>
            <td style="font-family: Arial, sans-serif; font-size: 12px; padding: 10px 5px; overflow: hidden; word-break: normal; border-color: black;" colspan="2" align="right">
              <h2>Invoice</h2>
            </td>
          </tr>
          <tr>
            <td class="tg-0lax">
              <table class="tg" style="border-collapse: collapse; border-spacing: 0; width: 100%">
                <tr>
                  <td style="font-family: Arial, sans-serif; font-size: 12px; padding: 10px 5px; overflow: hidden; word-break: normal; border-color: black;" class="tg-0lax">
                    <img src="{LogoUrl}" width="50%" />
                  </td>
                </tr>
                <tr>
                  <td>
                    <table>
                      <tr>
                        <td class="">
                          <xsl:value-of select="DeepwellAddress/WellName" />
                        </td>
                      </tr>
                      <tr>
                        <td class="">
                          <xsl:value-of select="DeepwellAddress/AddressLine2" />
                        </td>
                      </tr>
                      <tr>
                        <td class="">
                          <xsl:value-of select="DeepwellAddress/AddressLine3" />
                        </td>
                      </tr>
                      <tr>
                        <td class="">
                          <xsl:value-of select="DeepwellAddress/AddressLine4" />
                        </td>
                      </tr>
                    </table>
                  </td>
                </tr>
              </table>
            </td>
            <td class="tg-0lax">
              <table border="1" class="tg" style="border-collapse: collapse; border-spacing: 0; width: 100%">
                <tr>
                  <th style="background-color: lightgray; align-content: center; font-family: Arial, sans-serif; font-size: 12px; font-weight: bold; padding: 10px 5px; border-style: solid; border-width: 1px; overflow: hidden; word-break: normal; border-color: black;" class="tg-0lax">DATE</th>
                  <th style="background-color: lightgray; align-content: center; font-family: Arial, sans-serif; font-size: 12px; font-weight: bold; padding: 10px 5px; border-style: solid; border-width: 1px; overflow: hidden; word-break: normal; border-color: black;" class="tg-0lax">INVOICE #</th>
                </tr>
                <tr>
                  <td align="center" style="font-family: Arial, sans-serif; font-size: 12px; padding: 10px 5px; border-style: solid; border-width: 1px; overflow: hidden; word-break: normal; border-color: black;" class="tg-0lax">
                    <xsl:value-of select="Date" />
                  </td>
                  <td align="center" style="font-family: Arial, sans-serif; font-size: 12px; padding: 10px 5px; border-style: solid; border-width: 1px; overflow: hidden; word-break: normal; border-color: black;" class="tg-0lax">
                    <xsl:value-of select="InvoiceNumber" />
                  </td>
                </tr>
                <tr>
                  <th style="background-color: lightgray; align-content: center; font-family: Arial, sans-serif; font-size: 12px; font-weight: bold; padding: 10px 5px; border-style: solid; border-width: 1px; overflow: hidden; word-break: normal; border-color: black;" class="tg-0lax">TERMS</th>
                  <th style="background-color: lightgray; align-content: center; font-family: Arial, sans-serif; font-size: 12px; font-weight: bold; padding: 10px 5px; border-style: solid; border-width: 1px; overflow: hidden; word-break: normal; border-color: black;" class="tg-0lax">DUE DATE</th>
                </tr>
                <tr>
                  <td align="center" style="font-family: Arial, sans-serif; font-size: 12px; padding: 10px 5px; border-style: solid; border-width: 1px; overflow: hidden; word-break: normal; border-color: black;" class="tg-0lax">
                    <xsl:value-of select="Terms" />
                  </td>
                  <td align="center" style="font-family: Arial, sans-serif; font-size: 12px; padding: 10px 5px; border-style: solid; border-width: 1px; overflow: hidden; word-break: normal; border-color: black;" class="tg-0lax">
                    <xsl:value-of select="DueDate" />
                  </td>
                </tr>
              </table>
            </td>
          </tr>
          <tr>
            <td style="font-family: Arial, sans-serif; font-size: 12px; padding: 10px 5px; overflow: hidden; word-break: normal; border-color: black;" class="tg-0lax">
              <table class="tg" style="border-collapse: collapse; border-spacing: 0; width: 100%">
                <tr>
                  <th style="background-color: lightgray; align-content: center; font-family: Arial, sans-serif; font-size: 12px; font-weight: bold; padding: 10px 5px; border-style: solid; border-width: 1px; overflow: hidden; word-break: normal; border-color: black;" class="tg-0lax">BILL TO</th>
                </tr>
                <tr>
                  <td style="font-family: Arial, sans-serif; font-size: 12px; padding: 10px 5px; border-style: solid; border-width: 1px; overflow: hidden; word-break: normal; border-color: black;" class="tg-0lax">
                    <table >
                      <tr>
                        <td class="">
                          <xsl:value-of select="BillToAddress/WellName" />
                        </td>
                      </tr>
                      <tr>
                        <td class="">
                          <xsl:value-of select="BillToAddress/City" />,
                          <xsl:value-of select="BillToAddress/State" />
                          <xsl:value-of select="BillToAddress/PostalCode" />
                        </td>
                      </tr>
                      <tr>
                        <td class="">
                          <xsl:value-of select="BillToAddress/County" />
                        </td>
                      </tr>
                    </table>
                  </td>
                </tr>
              </table>
            </td>
            <td style="font-family: Arial, sans-serif; font-size: 12px; padding: 10px 5px; overflow: hidden; word-break: normal; border-color: black;" class="tg-0lax">
              <table class="tg" style="border-collapse: collapse; border-spacing: 0; width: 100%">
                <tr>
                  <th style="background-color: lightgray; align-content: center; font-family: Arial, sans-serif; font-size: 12px; font-weight: bold; padding: 10px 5px; border-style: solid; border-width: 1px; overflow: hidden; word-break: normal; border-color: black;" class="tg-0lax">SHIP TO</th>
                </tr>
                <tr>
                  <td style="font-family: Arial, sans-serif; font-size: 12px; padding: 10px 5px; border-style: solid; border-width: 1px; overflow: hidden; word-break: normal; border-color: black;" class="tg-0lax">
                    <table >
                      <tr>
                        <td class="">
                          <xsl:value-of select="ShipToAddress/WellName" />
                        </td>
                      </tr>
                      <tr>
                        <td class="">
                          <xsl:value-of select="ShipToAddress/City" />,
                          <xsl:value-of select="ShipToAddress/State" />
                          <xsl:value-of select="ShipToAddress/PostalCode" />
                        </td>
                      </tr>
                      <tr>
                        <td class="">
                          <xsl:value-of select="ShipToAddress/County" />
                        </td>
                      </tr>
                    </table>
                  </td>
                </tr>
              </table>
            </td>
          </tr>
          <tr>
            <td colspan="2">
              <table border="1" class="tg" style="border-collapse: collapse; border-spacing: 0; width: 100%">
                <tr>
                  <th style="background-color: lightgray; align-content: center; font-family: Arial, sans-serif; font-size: 12px; font-weight: bold; padding: 10px 5px; border-style: solid; border-width: 1px; overflow: hidden; word-break: normal; border-color: black;" class="tg-0lax">Item</th>
                  <th style="background-color: lightgray; align-content: center; font-family: Arial, sans-serif; font-size: 12px; font-weight: bold; padding: 10px 5px; border-style: solid; border-width: 1px; overflow: hidden; word-break: normal; border-color: black;" class="tg-0lax">Description</th>
                  <th style="background-color: lightgray; align-content: center; font-family: Arial, sans-serif; font-size: 12px; font-weight: bold; padding: 10px 5px; border-style: solid; border-width: 1px; overflow: hidden; word-break: normal; border-color: black;" class="tg-0lax">Quantity</th>
                  <th style="background-color: lightgray; align-content: center; font-family: Arial, sans-serif; font-size: 12px; font-weight: bold; padding: 10px 5px; border-style: solid; border-width: 1px; overflow: hidden; word-break: normal; border-color: black;" class="tg-0lax">UOM</th>
                  <xsl:if test="IsPriceToBeDisplayed = 'true'">
                    <th style="background-color: lightgray; align-content: center; font-family: Arial, sans-serif; font-size: 12px; font-weight: bold; padding: 10px 5px; border-style: solid; border-width: 1px; overflow: hidden; word-break: normal; border-color: black;" class="tg-0lax">Rate</th>
                  </xsl:if>
                  <th style="background-color: lightgray; align-content: center; font-family: Arial, sans-serif; font-size: 12px; font-weight: bold; padding: 10px 5px; border-style: solid; border-width: 1px; overflow: hidden; word-break: normal; border-color: black;" class="tg-0lax">Amount</th>
                </tr>
                <br/>
                <xsl:for-each select="Invoices/Invoice">
                  <tr>
                    <td width="20%" align="center" class="tg-0lax">
                      <xsl:value-of select="ProductNumber" />
                    </td>
                    <td width="35%" align="center" class="tg-0lax">
                      <xsl:value-of select="Description" />
                    </td>
                    <td align="center" class="tg-0lax">
                      <xsl:value-of select="Quantity" />
                    </td>
                    <td align="center" class="tg-0lax">
                      <xsl:value-of select="Uom" />
                    </td>
                    <xsl:if test="../../IsPriceToBeDisplayed = 'true'">
                      <td align="right" class="tg-0lax">
                        <xsl:value-of select="Rate" />
                      </td>
                    </xsl:if>
                    <td align="right" class="tg-0lax">
                      <xsl:value-of select="Amount" />
                    </td>
                  </tr>
                </xsl:for-each>
                <xsl:choose>
                  <xsl:when test="IsPriceToBeDisplayed = 'true'">
                    <tr>
                      <td colspan="5" align="right">SUBTOTAL</td>
                      <td align="right" class="tg-0lax">
                        <xsl:value-of select="SubTotal" />
                      </td>
                    </tr>
                    <xsl:if test="IsDisplayTaxField = 'true'">
                      <tr>
                        <td colspan="5" align="right">TAX PERCENTAGE</td>
                        <td align="right" class="tg-0lax">
                          <xsl:value-of select="TaxPercentage" />
                        </td>
                      </tr>
                      <tr>
                        <td colspan="5" align="right">TAX</td>
                        <td align="right" class="tg-0lax">
                          <xsl:value-of select="Tax" />
                        </td>
                      </tr>
                    </xsl:if>
                    <tr>
                      <td colspan="5" align="right">TOTAL</td>
                      <td align="right" class="tg-0lax">
                        <xsl:value-of select="Total" />
                      </td>
                    </tr>
                  </xsl:when>
                  <xsl:otherwise>
                    <tr>
                      <td colspan="4" align="right">SUBTOTAL</td>
                      <td align="right" class="tg-0lax">
                        <xsl:value-of select="SubTotal" />
                      </td>
                    </tr>
                    <tr>
                      <td colspan="4" align="right">TAX PERCENTAGE</td>
                      <td align="right" class="tg-0lax">
                        <xsl:value-of select="TaxPercentage" />
                      </td>
                    </tr>
                    <tr>
                      <td colspan="4" align="right">TAX</td>
                      <td align="right" class="tg-0lax">
                        <xsl:value-of select="Tax" />
                      </td>
                    </tr>
                    <tr>
                      <td colspan="4" align="right">TOTAL</td>
                      <td align="right" class="tg-0lax">
                        <xsl:value-of select="Total" />
                      </td>
                    </tr>
                  </xsl:otherwise>
                </xsl:choose>
              </table>
            </td>
          </tr>
        </table>
        <br/>
        <table>
          <tr>
            <td style="padding-top: .5em; padding-bottom: .5em;">Comments :</td>
            <td align="center" class="tg-0lax">
              <xsl:value-of select="Comments" />
            </td>
          </tr>
        </table>
      </body>
    </html>
  </xsl:template>
</xsl:stylesheet>